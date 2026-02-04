using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Model
{
	public class AuditLog
	{
		[Key]
		public int Id { get; set; }

		public string UserId { get; set; }

		public string Action { get; set; } // "Login" or "Logout"

		public DateTime Timestamp { get; set; }
	}
}