using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using System.Diagnostics;

namespace PetGrooming.Controllers
{
    public class HomeController : Controller
    {
        private readonly DB _context;

        public HomeController(DB context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalPets = await _context.Appointments.CountAsync();
            ViewBag.TotalEmployees = await _context.Doctors.CountAsync(d => d.IsActive) +
                            await _context.Staffs.CountAsync(s => s.IsActive);
            ViewBag.PetsWithDoctors = await _context.Appointments.CountAsync(p => p.DoctorId != null);
            ViewBag.PetsWithoutDoctors = await _context.Appointments.CountAsync(p => p.DoctorId == null);

            var recentPets = await _context.Appointments
                .Include(p => p.Doctor)
                .OrderByDescending(p => p.AppointmentDate)
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

