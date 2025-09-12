using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;                   // ✅ NEW
using PetGrooming.Models;
using PetGroomingSystem.Models.ViewModels;

namespace PetGroomingSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly DB _context;

        public AppointmentController(DB context)
        {
            _context = context;
        }

        // GET: Appointment
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
            return View(appointments);
        }

        // GET: Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Doctor)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // ===========================
        //  Booking requires login
        // ===========================

        // GET: Appointment/Create
        [Authorize]                                             // ✅ must be logged in
        public async Task<IActionResult> Create()
        {
            await LoadServicesDropDown();
            ViewData["PetTypes"] = GetPetTypeSelectList();

            // Prefill the email with the logged-in user's identity if available
            var model = new Appointment
            {
                Email = User?.Identity?.Name ?? string.Empty
            };
            return View(model);
        }

        // POST: Appointment/Create
        [Authorize]                                             // ✅ must be logged in
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OwnerName,PetName,PetType,PetBreed,Age,Gender,Email,PhoneNumber,ServiceId,ServiceType,AppointmentDate,Notes")] Appointment appointment)
        {
            // Force email to the signed-in user to prevent spoofing
            if (User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                appointment.Email = User.Identity.Name!;
            }

            // ---------- Server-side phone normalization + validation ----------
            var digits = Regex.Replace(appointment.PhoneNumber ?? "", @"\D", "");
            appointment.PhoneNumber = digits;

            if (!Regex.IsMatch(digits, @"^01\d{8,9}$"))
            {
                ModelState.AddModelError("PhoneNumber", "Please enter a valid mobile number (starts with 01, 10–11 digits).");
            }

            // ---------- Validate appointment in the future ----------
            if (appointment.AppointmentDate <= DateTime.Now)
            {
                ModelState.AddModelError("AppointmentDate", "Appointment date must be in the future.");
            }

            // ---------- Working hours validation (Mon–Fri 08:00–18:00, Sat 09:00–16:00) ----------
            var dt = appointment.AppointmentDate;
            bool inBusinessHours =
                (dt.DayOfWeek >= DayOfWeek.Monday && dt.DayOfWeek <= DayOfWeek.Friday && dt.TimeOfDay >= TimeSpan.FromHours(8) && dt.TimeOfDay <= TimeSpan.FromHours(18)) ||
                (dt.DayOfWeek == DayOfWeek.Saturday && dt.TimeOfDay >= TimeSpan.FromHours(9) && dt.TimeOfDay <= TimeSpan.FromHours(16));

            if (!inBusinessHours)
            {
                ModelState.AddModelError("AppointmentDate", "Selected time is outside business hours.");
            }

            // ---------- Require notes when "Other / Custom Request" ----------
            var chosenService = await _context.Services.FindAsync(appointment.ServiceId);
            if (chosenService != null && chosenService.Name == "Other / Custom Request")
            {
                if (string.IsNullOrWhiteSpace(appointment.Notes) || appointment.Notes.Trim().Length < 10)
                {
                    ModelState.AddModelError("Notes", "Please describe your custom request (at least 10 characters).");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadServicesDropDown(appointment.ServiceId);
                ViewData["PetTypes"] = GetPetTypeSelectList();
                return View(appointment);
            }

            // Save
            appointment.CreatedDate = DateTime.Now;
            _context.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully!";

            // After booking, go straight to the user's list filtered by their email
            return RedirectToAction(nameof(My), new { email = appointment.Email });
        }

        // GET: Appointment/Confirmation/5
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // GET: /Appointment/My
        // Shows Upcoming and History, filtered by email when provided (or by logged-in user if available).
        [Authorize]                                             // ✅ require login to view personal list
        public async Task<IActionResult> My(string? email = null)
        {
            var filterEmail = email ?? User?.Identity?.Name;
            var now = DateTime.Now;

            var query = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Service)
                .Include(a => a.Doctor)
                .Include(a => a.Staff)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterEmail))
            {
                query = query.Where(a => a.Email == filterEmail);
                ViewData["FilterEmail"] = filterEmail;
            }
            else
            {
                // no email available: show nothing to avoid exposing other users' data
                query = query.Where(a => false);
                ViewData["FilterEmail"] = null;
            }

            var all = await query.OrderBy(a => a.AppointmentDate).ToListAsync();

            var vm = new AppointmentListVM
            {
                Email = filterEmail,
                Upcoming = all.Where(a => a.AppointmentDate >= now)
                              .OrderBy(a => a.AppointmentDate).ToList(),
                History = all.Where(a => a.AppointmentDate < now)
                             .OrderByDescending(a => a.AppointmentDate).ToList()
            };

            return View(vm);
        }

        // -------------------------
        // Helpers
        // -------------------------

        private async Task LoadServicesDropDown(int? selectedId = null)
        {
            var services = await _context.Services
                .ToListAsync();

            var items = services
                .OrderBy(s => s.Name == "Other / Custom Request" ? 1 : 0) // put "Other" last
                .ThenBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Name} (RM{s.Price:0.00})"
                })
                .ToList();

            ViewBag.ServiceId = new SelectList(items, "Value", "Text", selectedId?.ToString());
        }

        private SelectList GetPetTypeSelectList()
        {
            var petTypes = new[]
            {
                new SelectListItem { Value = "Dog", Text = "Dog" },
                new SelectListItem { Value = "Cat", Text = "Cat" },
                new SelectListItem { Value = "Rabbit", Text = "Rabbit" },
                new SelectListItem { Value = "Guinea Pig", Text = "Guinea Pig" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };
            return new SelectList(petTypes, "Value", "Text");
        }
    }
}
