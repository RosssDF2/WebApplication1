using System.Text.Encodings.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class RegisterModel : PageModel
    {
        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }
        private IWebHostEnvironment _environment;
        private IDataProtector _protector;

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment,
            IDataProtectionProvider provider)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _environment = environment;
            _protector = provider.CreateProtector("CreditCardProtector");
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // 1. Photo Validation
                var extension = Path.GetExtension(RModel.Photo.FileName).ToLower();
                if (extension != ".jpg")
                {
                    ModelState.AddModelError("RModel.Photo", "Only .jpg images are allowed.");
                    return Page();
                }

                // 2. Save Photo
                var imageFile = Guid.NewGuid().ToString() + extension;
                var folderPath = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, imageFile);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await RModel.Photo.CopyToAsync(fileStream);

                var user = new ApplicationUser()
                {
                    UserName = RModel.Email,
                    Email = RModel.Email,
                    FullName = HtmlEncoder.Default.Encode(RModel.FullName), // Encodes potential XSS
                    CreditCard = _protector.Protect(RModel.CreditCard),
                    Gender = RModel.Gender,
                    MobileNo = RModel.MobileNo,

                    // Sanitize multi-line text areas
                    DeliveryAddress = HtmlEncoder.Default.Encode(RModel.DeliveryAddress),
                    AboutMe = HtmlEncoder.Default.Encode(RModel.AboutMe),

                    PhotoPath = "/uploads/" + imageFile,
                    LastPasswordChangedDate = DateTime.Now,
                    TwoFactorEnabled = true
                };

                // 4. Save to Database
                var result = await userManager.CreateAsync(user, RModel.Password);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
                    return RedirectToPage("Index");
                }

                foreach (var error in result.Errors)
                {
                    // Check if the error is about the Duplicate Username
                    if (error.Code == "DuplicateUserName")
                    {
                        // Swallow the default message and show your own
                        ModelState.AddModelError("RModel.Email", "This Email address is already taken. Please try another.");
                    }
                    else
                    {
                        // Show other errors (like Password too weak) normally
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return Page();
        }
    }
}