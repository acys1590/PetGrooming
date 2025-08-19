using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetGroomingSystem.Data;
using System.Diagnostics;

namespace PetGroomingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalPets = await _context.Pets.CountAsync();
            ViewBag.TotalDoctors = await _context.Doctors.CountAsync(d => d.IsActive);
            ViewBag.PetsWithDoctors = await _context.Pets.CountAsync(p => p.DoctorId != null);
            ViewBag.PetsWithoutDoctors = await _context.Pets.CountAsync(p => p.DoctorId == null);

            var recentPets = await _context.Pets
                .Include(p => p.Doctor)
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .ToListAsync();

            return View(recentPets);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string RequestId { get; set; } = string.Empty;
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

}

