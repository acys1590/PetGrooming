using Microsoft.EntityFrameworkCore;
using PetGrooming;
using PetGroomingSystem.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSqlServer<DB>($@"
 Data Source=(LocalDB)\MSSQLLocalDB;
 AttachDbFilename={builder.Environment.ContentRootPath}\Db.mdf;
");

//// Add Entity Framework - Note the class name is now 'DB'
//builder.Services.AddDbContext<DB>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 注册 HttpContextAccessor（给 Helper 用）
builder.Services.AddHttpContextAccessor();

// 如果 Helper / Controller 要用 Session，这里也要启用
builder.Services.AddSession();

// 注册 Helper
builder.Services.AddScoped<Helper>();

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

app.UseAuthorization();

// ⚠️ 如果启用 Session，必须加这一行
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();