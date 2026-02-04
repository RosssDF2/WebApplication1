using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
	public class Register
	{
		[Required]
		public string FullName { get; set; }

		[Required]
		[DataType(DataType.CreditCard)]
		public string CreditCard { get; set; }

		[Required]
		public string Gender { get; set; }

		[Required]
		[RegularExpression(@"^[89]\d{7}$", ErrorMessage = "Invalid Mobile Number")]
		public string MobileNo { get; set; }

		[Required]
		public string DeliveryAddress { get; set; }

		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
		public string ConfirmPassword { get; set; }

		[Required]
		public IFormFile Photo { get; set; }

		[Required]
		public string AboutMe { get; set; }
	}
}