using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Model;

namespace WebApplication1.Pages
{
    [Authorize] // 1. Force user to login to see this page
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDataProtector _protector;
        private readonly AuthDbContext _context;
        public ApplicationUser CurrentUser { get; set; }
        public string DecryptedCreditCard { get; set; }

        public IndexModel(UserManager<ApplicationUser> userManager,
                           IDataProtectionProvider provider,
                           AuthDbContext context)
        {
            _userManager = userManager;
            _context = context; // 3. INITIALIZE CONTEXT
            _protector = provider.CreateProtector("CreditCardProtector");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            CurrentUser = user;

            try
            {
                DecryptedCreditCard = _protector.Unprotect(user.CreditCard);
            }
            catch
            {
                DecryptedCreditCard = "Error decrypting card";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostToggle2FAAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            // 1. Toggle the 2FA status
            user.TwoFactorEnabled = !user.TwoFactorEnabled;

            // 2. FORCE EMAIL CONFIRMATION (The missing piece!)
            // If we are enabling 2FA, we must ensure the email is confirmed
            // otherwise Identity won't generate the token.
            if (user.TwoFactorEnabled)
            {
                user.EmailConfirmed = true;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var auditLog = new AuditLog
                {
                    UserId = user.Email,
                    Action = user.TwoFactorEnabled ? "Enabled 2FA" : "Disabled 2FA",
                    Timestamp = DateTime.Now
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Add this to your OnPost handlers in Index.cshtml.cs

    }
}