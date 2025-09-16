using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using PetGroomingSystem;
using PetGroomingSystem.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ===== MVC + Localization =====
builder.Services.AddLocalization();

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// ===== EF Core (DefaultConnection only) =====
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DB>(opts => opts.UseSqlServer(conn));

// ===== HttpContext / Session / Helpers =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddScoped<HelperBase>();

// ===== Email Sender =====
builder.Services.AddScoped<IEmailSender, EmailSender>();

// ===== Cookie Authentication =====
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Accounts/Login";
        options.LogoutPath = "/Accounts/Logout";
        options.AccessDeniedPath = "/Accounts/AccessDenied";
    });

var app = builder.Build();

// ===== Ensure DB exists =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DB>();
    context.Database.EnsureCreated();
}

// ===== Request Localization (EN + BM) =====
var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("ms-MY")
};

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US", "ms-MY")
    .AddSupportedUICultures("en-US", "ms-MY");

// Prefer cookie over Accept-Language
localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());

// ===== Pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 🚨 Move localization BEFORE routing
app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}");

app.Run();
