using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace PetGroomingSystem;

public class HelperBase
{
    private readonly IWebHostEnvironment en;
    private readonly IHttpContextAccessor ct;

    public HelperBase(IWebHostEnvironment en, IHttpContextAccessor ct)
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

        string uploadPath = Path.Combine(en.WebRootPath, folder);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
        string filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            photo.CopyTo(stream);
        }

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

    // ✅ 修复 SignIn：明确指定 CookieAuthenticationDefaults.AuthenticationScheme
    internal async Task SignIn(string email, string role, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await ct.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            });
    }

    // ✅ 修复 SignOut：同样指定 scheme
    internal async Task SignOut()
    {
        await ct.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    internal string RandomPassword()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    internal bool VerifyPassword(object hash, string current)
    {
        throw new NotImplementedException();
    }
}
