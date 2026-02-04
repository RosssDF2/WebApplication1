using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;

namespace WebApplication1.Pages
{
	public class LogoutModel : PageModel
	{
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly AuthDbContext _context;

		public LogoutModel(SignInManager<ApplicationUser> signInManager, AuthDbContext context)
		{
			this.signInManager = signInManager;
			this._context = context;
		}

		public void OnGet() { }

		public async Task<IActionResult> OnPostLogoutAsync()
		{
			// Log the logout action before signing out
			if (User.Identity.IsAuthenticated)
			{
				var auditLog = new AuditLog
				{
					UserId = User.Identity.Name, // Using the email/username
					Action = "Logout",
					Timestamp = DateTime.Now
				};
				_context.AuditLogs.Add(auditLog);
				await _context.SaveChangesAsync();
			}

			await signInManager.SignOutAsync();
			return RedirectToPage("Login");
		}

		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}
	}
}