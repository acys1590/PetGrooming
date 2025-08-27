using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace PetGroomingSystem;

public class Helper : HelperBase
{
    private readonly IWebHostEnvironment en;
    private readonly IHttpContextAccessor ct;

    public Helper(IWebHostEnvironment en, IHttpContextAccessor ct)
    {
        this.en = en;
        this.ct = ct;
    }

    // ------------------------------------------------------------------------
    // Photo Upload
    // ------------------------------------------------------------------------

    public string ValidatePhoto(IFormFile f)
    {
        var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
        var reName = new Regex(@"^.+\.(jpeg|jpg|png)$", RegexOptions.IgnoreCase);

        if (!reType.IsMatch(f.ContentType) || !reName.IsMatch(f.FileName))
        {
            return "Only JPG and PNG photo is allowed.";
        }
        else if (f.Length > 1 * 1024 * 1024)
        {
            return "Photo size cannot be more than 1MB.";
        }

        return "";
    }

    internal string SavePhoto(IFormFile photo, string folder)
    {
        if (photo == null || photo.Length == 0) return "";

        // 确保目录存在
        string uploadPath = Path.Combine(en.WebRootPath, folder);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // 生成唯一文件名
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
        string filePath = Path.Combine(uploadPath, fileName);

        // 保存文件
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            photo.CopyTo(stream);
        }

        // 返回相对路径，方便存数据库
        return $"/{folder}/{fileName}";
    }

    internal void DeletePhoto(string photoURL, string folder)
    {
        if (string.IsNullOrEmpty(photoURL)) return;

        string filePath = Path.Combine(en.WebRootPath, folder, Path.GetFileName(photoURL));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    internal string HashPassword(string password)
    {
        var hasher = new PasswordHasher<string>();
        return hasher.HashPassword(null, password);
    }

    internal bool VerifyPassword(string hash, string password)
    {
        var hasher = new PasswordHasher<string>();
        var result = hasher.VerifyHashedPassword(null, hash, password);
        return result == PasswordVerificationResult.Success;
    }

    internal void SignIn(string email, string role, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "login");
        var principal = new ClaimsPrincipal(identity);

        ct.HttpContext.SignInAsync(principal, new AuthenticationProperties
        {
            IsPersistent = rememberMe
        }).Wait();
    }

    internal void SignOut()
    {
        ct.HttpContext.SignOutAsync().Wait();
    }

    internal string RandomPassword()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8);
    }
}
