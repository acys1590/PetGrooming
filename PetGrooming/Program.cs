using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using PetGroomingSystem;
using PetGroomingSystem.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ===== MVC + Localization =====
builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// ===== EF Core (use your DefaultConnection only; removed duplicate) =====
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DB>(opts => opts.UseSqlServer(conn));

// ===== HttpContext / Session / Helpers =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddScoped<HelperBase>();

// ===== Email Sender (yours) =====
builder.Services.AddScoped<IEmailSender, EmailSender>();

// ===== Cookie Auth (yours) =====
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Accounts/Login";
        options.LogoutPath = "/Accounts/Logout";
        options.AccessDeniedPath = "/Accounts/AccessDenied";
    });

var app = builder.Build();

// Ensure DB exists (your behavior)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DB>();
    context.Database.EnsureCreated();
}

// ===== Pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ===== Request Localization (EN + BM) =====
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US", "ms-MY")
    .AddSupportedUICultures("en-US", "ms-MY");

// Prefer culture from cookie first
localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}");

app.Run();
