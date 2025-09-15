using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using PetGroomingSystem;
using PetGroomingSystem.Services;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSqlServer<DB>($@"
 Data Source=(LocalDB)\MSSQLLocalDB;
 AttachDbFilename={builder.Environment.ContentRootPath}\Db.mdf;
");
// 注册 HttpContextAccessor（给 Helper 用）
builder.Services.AddHttpContextAccessor();
// 如果 Helper / Controller 要用 Session，这里也要启用
builder.Services.AddSession();
// 注册 Helper
builder.Services.AddScoped<HelperBase>();
// ✅ 注册 Cookie 认证
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Accounts/Login";             // 未登录时跳转
        options.LogoutPath = "/Accounts/Logout";           // 登出路径
        options.AccessDeniedPath = "/Accounts/AccessDenied"; // 无权限跳转
    });

builder.Services.AddScoped<IEmailSender, EmailSender>();

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DB>(opts => opts.UseSqlServer(conn));


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DB>();
    context.Database.EnsureCreated();
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();   // ✅ 认证要放在 Authorization 前
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();