using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vonage.Server;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
