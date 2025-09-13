using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Member")]
public class Member1Controller : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}