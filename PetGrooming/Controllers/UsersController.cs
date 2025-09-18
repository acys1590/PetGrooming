using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetGrooming.Models;   // 注意这里是 PetGrooming.Models (因为 DB 在这个 namespace)

//[Authorize(Roles = "Admin")]
[AllowAnonymous]
public class UsersController : Controller
{
    private readonly DB _context;

    public UsersController(DB context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var users = _context.Users.ToList();
        return View(users);
    }

    [HttpPost]
    public IActionResult Unlock(string email)   // 用 Email 作为主键
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user != null)
        {
            user.IsLocked = false;
            user.FailedAttempts = 0;
            _context.SaveChanges();
            TempData["Success"] = $"User {user.Email} has been unlocked.";
        }
        else
        {
            TempData["Error"] = "User not found.";
        }

        return RedirectToAction("Index");
    }
}
