using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;

namespace WebApplication1.Pages
{
    public class LoginTwoStepModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;

        public LoginTwoStepModel(SignInManager<ApplicationUser> signInManager,
                                 UserManager<ApplicationUser> userManager,
                                 AuthDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public string TwoFactorCode { get; set; }

        [BindProperty]
        public bool RememberMe { get; set; }

        public async Task<IActionResult> OnGetAsync(bool rememberMe)
        {
            // Ensure we have a user in the "2FA process" state
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToPage("Login");
            }
            RememberMe = rememberMe;

            // Generate Code
            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            if (providers.Contains("Email"))
            {
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                // SIMULATE EMAIL SENDING
                System.Diagnostics.Debug.WriteLine($"{code}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return RedirectToPage("Login");

            // Verify Code
            var result = await _signInManager.TwoFactorSignInAsync("Email", TwoFactorCode, RememberMe, false);

            if (result.Succeeded)
            {
                // 1. UPDATE SECURITY STAMP (This invalidates the OTHER browser)
                await _userManager.UpdateSecurityStampAsync(user);

                // 2. RE-SIGN IN (This ensures THIS browser gets the new valid cookie)
                // Since we just verified 2FA, we can manually sign them in with the new stamp
                await _signInManager.SignInAsync(user, RememberMe);

                // 3. Create Audit Log
                var auditLog = new AuditLog
                {
                    UserId = user.Email,
                    Action = "Login (2FA)",
                    Timestamp = DateTime.Now
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                return RedirectToPage("Index");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Account is locked out.");
                return Page();
            }

            ModelState.AddModelError("", "Invalid Code.");
            return Page();
        }
    }
}