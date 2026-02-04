using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Model;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class ChangePasswordModel : PageModel
    {
        [BindProperty]
        public ChangePassword CPModel { get; set; }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuthDbContext _context;

        public ChangePasswordModel(UserManager<ApplicationUser> userManager,
                                   SignInManager<ApplicationUser> signInManager,
                                   AuthDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToPage("/Login");

                // 1. MINIMUM PASSWORD AGE CHECK
                // Example: You cannot change password if you changed it less than 20 mins ago
                var minPasswordAge = TimeSpan.FromMinutes(0);
                if (DateTime.Now < user.LastPasswordChangedDate.Add(minPasswordAge))
                {
                    ModelState.AddModelError("", "You can only change your password once every 20 minutes.");
                    return Page();
                }

                // 2. PASSWORD HISTORY CHECK (Cannot reuse last 2 passwords)
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                var passwordHistory = await _context.PasswordHistories
                    .Where(ph => ph.UserId == user.Id)
                    .OrderByDescending(ph => ph.CreatedDate)
                    .Take(2) // Check last 2 passwords
                    .ToListAsync();

                foreach (var record in passwordHistory)
                {
                    if (passwordHasher.VerifyHashedPassword(user, record.PasswordHash, CPModel.NewPassword) != PasswordVerificationResult.Failed)
                    {
                        ModelState.AddModelError("", "You cannot reuse your recent passwords.");
                        return Page();
                    }
                }

                // 3. CHANGE PASSWORD
                var result = await _userManager.ChangePasswordAsync(user, CPModel.CurrentPassword, CPModel.NewPassword);
                if (result.Succeeded)
                {
                    // 4. ADD TO HISTORY
                    var historyEntry = new PasswordHistory
                    {
                        UserId = user.Id,
                        PasswordHash = user.PasswordHash, // Save the OLD hash (before we refresh sign-in)
                        CreatedDate = DateTime.Now
                    };
                    _context.PasswordHistories.Add(historyEntry);

                    // 5. UPDATE USER DATE
                    user.LastPasswordChangedDate = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                    await _context.SaveChangesAsync();

                    // 6. REFRESH SIGN-IN (Keep user logged in)
                    await _signInManager.RefreshSignInAsync(user);

                    return RedirectToPage("Index");
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