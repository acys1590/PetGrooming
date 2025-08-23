using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.Security.Claims;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

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


    internal void DeletePhoto(string photoURL, string v)
    {
        throw new NotImplementedException();
    }

    internal string HashPassword(string password)
    {
        throw new NotImplementedException();
    }

    internal string HashPassword(object @new)
    {
        throw new NotImplementedException();
    }

    internal string RandomPassword()
    {
        throw new NotImplementedException();
    }

    internal string SavePhoto(IFormFile photo, string v)
    {
        throw new NotImplementedException();
    }

    internal void SignIn(string email, string role, bool rememberMe)
    {
        throw new NotImplementedException();
    }

    internal void SignOut()
    {
        throw new NotImplementedException();
    }

    internal string ValidatePhoto(object photo)
    {
        throw new NotImplementedException();
    }

    internal bool VerifyPassword(string hash, string current)
    {
        throw new NotImplementedException();
    }

    internal bool VerifyPassword(string hash, object current)
    {
        throw new NotImplementedException();
    }

    internal bool VerifyPassword(object hash, string password)
    {
        throw new NotImplementedException();
    }
}
