using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public ResetPassword RPModel { get; set; }

        public IActionResult OnGet(string token, string email)
        {
            if (token == null || email == null)
            {
                return RedirectToPage("/Login");
            }
            // Pre-fill the model with the token and email from the URL
            RPModel = new ResetPassword { Token = token, Email = email };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(RPModel.Email);
                if (user == null) return RedirectToPage("/Login");

                // Reset the password using the token
                var result = await _userManager.ResetPasswordAsync(user, RPModel.Token, RPModel.Password);

                if (result.Succeeded)
                {
                    // Optional: Reset their "Last Password Changed Date" here too if you want
                    TempData["Message"] = "Password reset successful. You can now login.";
                    return RedirectToPage("/Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return Page();
        }
    }
}