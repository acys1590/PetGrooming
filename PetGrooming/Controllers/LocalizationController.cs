using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace PetGroomingSystem.Controllers
{
    public class LocalizationController : Controller
    {
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl = "/")
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            // basic safety: only allow local redirects
            if (!Url.IsLocalUrl(returnUrl)) return RedirectToAction("Index", "Main");
            return LocalRedirect(returnUrl);
        }
    }
}
