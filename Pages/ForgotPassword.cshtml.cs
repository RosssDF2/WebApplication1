using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;

namespace WebApplication1.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ForgotPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public string Email { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Email);
                if (user != null)
                {
                    // 1. Generate Password Reset Token
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    // 2. Create the Reset Link
                    var resetLink = Url.Page("/ResetPassword",
                        pageHandler: null,
                        values: new { token = token, email = Email },
                        protocol: Request.Scheme);

                    // 3. SIMULATE SENDING EMAIL (Check Visual Studio Output Window)
                    // In a real app, you would send this via SMTP
                    System.Diagnostics.Debug.WriteLine($"{resetLink}");

                    // Show success message regardless (Security Best Practice: Don't reveal if email exists)
                    TempData["Message"] = "If an account exists, a reset link has been sent.";
                    return Page();
                }
            }
            return Page();
        }
    }
}