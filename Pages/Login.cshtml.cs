using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.ViewModels;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public string StatusMessage { get; set; }

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager,
                          AuthDbContext context,
                          IConfiguration configuration)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this._context = context;
            this._configuration = configuration;
        }

        public void OnGet(string returnUrl = null)
        {
            // Detect if user was redirected due to an invalidated security stamp
            if (!string.IsNullOrEmpty(returnUrl))
            {
                StatusMessage = "You have been logged out because your session is no longer valid or you logged in from another device.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // 1. reCAPTCHA v3 Validation
                var captchaResponse = Request.Form["g-recaptcha-response"];
                var secretKey = _configuration["GoogleReCaptcha:SecretKey"];

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}", null);
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var captchaResult = JsonSerializer.Deserialize<JsonNode>(jsonResponse);

                    if (captchaResult["success"]?.GetValue<bool>() == false || captchaResult["score"]?.GetValue<double>() < 0.5)
                    {
                        ModelState.AddModelError("", "reCAPTCHA validation failed. Please try again.");
                        return Page();
                    }
                }

                // 2. ATTEMPT LOGIN
                // lockoutOnFailure: true enables the 3-attempt lockout policy
                var result = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, lockoutOnFailure: true);

                // 3. HANDLE LOCKOUT
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Your account is locked out. Please wait for 15 minutes.");
                    return Page();
                }

                // 4. HANDLE 2FA
                // This will trigger if TwoFactorEnabled is true and Email is confirmed in DB
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("LoginTwoStep", new { rememberMe = LModel.RememberMe });
                }

                // 5. HANDLE SUCCESS
                if (result.Succeeded)
                {
                    var user = await userManager.FindByEmailAsync(LModel.Email);

                    // --- A. Max Password Age Check (5 Minutes) ---
                    var maxPasswordAge = TimeSpan.FromMinutes(5);
                    if (user.LastPasswordChangedDate.Add(maxPasswordAge) < DateTime.Now)
                    {
                        return RedirectToPage("ChangePassword");
                    }

                    // --- B. Single Session Enforcement ---
                    // Invalidates other browser sessions by changing the security stamp
                    await userManager.UpdateSecurityStampAsync(user);
                    await signInManager.SignInAsync(user, LModel.RememberMe);

                    // --- C. Audit Log ---
                    var auditLog = new AuditLog
                    {
                        UserId = user.Email,
                        Action = "Login",
                        Timestamp = DateTime.Now
                    };
                    _context.AuditLogs.Add(auditLog);
                    await _context.SaveChangesAsync();

                    return RedirectToPage("Index");
                }

                // 6. HANDLE FAILURE
                ModelState.AddModelError("", "Username or Password incorrect");
            }
            return Page();
        }
    }
}