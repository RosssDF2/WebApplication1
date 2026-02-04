using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string CreditCard { get; set; }
        public string Gender { get; set; }
        public string MobileNo { get; set; }
        public string DeliveryAddress { get; set; }
        public string? PhotoPath { get; set; }
        public string AboutMe { get; set; }

        public int? PasswordAge { get; set; } // Tracks how old the password is (optional logic)
        public DateTime LastPasswordChangedDate { get; set; } // For "Max Password Age" policy
    }
}