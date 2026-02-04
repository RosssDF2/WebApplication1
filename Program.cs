using Microsoft.AspNetCore.Identity;
using WebApplication1.Model;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// 1. SERVICE REGISTRATION (BEFORE Build)
// ==============================

builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders(); // <--- FIXED: This enables password reset tokens

builder.Services.AddDataProtection();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
});

// IMPORTANT: Tell Identity to check the security stamp frequently
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Check if the security stamp has changed every 0 seconds (Immediate)
    options.ValidationInterval = TimeSpan.Zero;
});

// ==============================
// 2. BUILD THE APP
// ==============================
var app = builder.Build();

// ==============================
// 3. MIDDLEWARE PIPELINE (AFTER Build)
// ==============================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/Errors/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();