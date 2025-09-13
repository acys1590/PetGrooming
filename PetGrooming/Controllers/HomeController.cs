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
            var petsWithEmployees = _context.Appointments
            .Count(a => a.DoctorId != null || a.StaffId != null);

            var petsWithoutEmployees = _context.Appointments
                .Count(a => a.DoctorId == null && a.StaffId == null);

            ViewBag.PetsWithEmployees = petsWithEmployees;
            ViewBag.PetsWithoutEmployees = petsWithoutEmployees;

            ViewBag.TotalPets = _context.Appointments.Count();
            ViewBag.TotalEmployees = _context.Doctors.Count(d => d.IsActive) + _context.Staffs.Count(s => s.IsActive);

            var appointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .ToListAsync();

            // Group pets handled by Doctor/Staff
            var handledPets = appointments
                .GroupBy(a => a.Doctor != null ? $"Dr. {a.Doctor.Name}" :
                              a.Staff != null ? $"Staff {a.Staff.Name}" :
                              "Unassigned")
                .Select(g => new
                {
                    Handler = g.Key,
                    Count = g.Count()
                }).ToList();

            ViewBag.Handlers = handledPets.Select(h => h.Handler).ToList();
            ViewBag.Counts = handledPets.Select(h => h.Count).ToList();

            return View(appointments);
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

