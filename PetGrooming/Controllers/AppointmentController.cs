using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
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

        // Admin/listing
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
            return View(appointments);
        }

        // GET: Appointment/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await LoadServicesDropDown();
            ViewData["PetTypes"] = GetPetTypeSelectList();

            // Server-side default to today's next 15-min slot
            var now = DateTime.Now;
            var minutesToAdd = (15 - (now.Minute % 15)) % 15;
            if (minutesToAdd == 0) minutesToAdd = 15;
            var nextQuarter = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0)
                .AddMinutes(minutesToAdd);

            var model = new Appointment
            {
                Email = User?.Identity?.Name ?? string.Empty,
                AppointmentDate = nextQuarter
            };

            return View(model);
        }

        // POST: Appointment/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OwnerName,PetName,PetType,PetBreed,Age,Gender,Email,PhoneNumber,ServiceId,ServiceType,AppointmentDate,Notes")] Appointment appointment)
        {
            // Force logged-in user email
            if (User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                appointment.Email = User.Identity.Name!;
            }

            // === Server-side validations ===

            // (A) Age: 0–99 (max 2 digits)
            if (appointment.Age.HasValue)
            {
                if (appointment.Age.Value < 0 || appointment.Age.Value > 99)
                {
                    ModelState.AddModelError("Age", "Age must be between 0 and 99.");
                }
            }

            // (B) Phone (MY mobile: starts with 01 + 8–9 digits)
            var digits = Regex.Replace(appointment.PhoneNumber ?? "", @"\D", "");
            appointment.PhoneNumber = digits;
            if (!Regex.IsMatch(digits, @"^01\d{8,9}$"))
            {
                ModelState.AddModelError("PhoneNumber", "Please enter a valid mobile number (starts with 01, 10–11 digits).");
            }

            // (C) AppointmentDate must be in future & within business hours
            if (appointment.AppointmentDate <= DateTime.Now)
            {
                ModelState.AddModelError("AppointmentDate", "Appointment date must be in the future.");
            }
            var dt = appointment.AppointmentDate;
            bool inBusinessHours =
                (dt.DayOfWeek >= DayOfWeek.Monday && dt.DayOfWeek <= DayOfWeek.Friday && dt.TimeOfDay >= TimeSpan.FromHours(8) && dt.TimeOfDay <= TimeSpan.FromHours(18)) ||
                (dt.DayOfWeek == DayOfWeek.Saturday && dt.TimeOfDay >= TimeSpan.FromHours(9) && dt.TimeOfDay <= TimeSpan.FromHours(16));
            if (!inBusinessHours)
            {
                ModelState.AddModelError("AppointmentDate", "Selected time is outside business hours.");
            }

            // (D) Notes required for "Other / Custom Request"
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

            // Neutral confirmation (success only after payment)
            return RedirectToAction(nameof(Confirmation), new { id = appointment.Id });
        }

        // GET: Appointment/Confirmation/5
        public IActionResult Confirmation(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // View my appointments (Upcoming + History)
        [Authorize]
        public async Task<IActionResult> My(string? email = null)
        {
            var filterEmail = email ?? User?.Identity?.Name;
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
                query = query.Where(a => false);
            }

            var now = DateTime.Now;
            var all = await query.OrderBy(a => a.AppointmentDate).ToListAsync();

            var vm = new AppointmentListVM
            {
                Email = filterEmail,
                Upcoming = all.Where(a => a.AppointmentDate >= now).OrderBy(a => a.AppointmentDate).ToList(),
                History = all.Where(a => a.AppointmentDate < now).OrderByDescending(a => a.AppointmentDate).ToList()
            };

            return View(vm);
        }

        // Optional payment jump
        [Authorize]
        public IActionResult Pay(int id)
        {
            var appt = _context.Appointments.AsNoTracking().FirstOrDefault(a => a.Id == id);
            if (appt == null) return NotFound();
            return RedirectToAction("Index", "Payment", new { serviceId = appt.ServiceId, appointmentId = appt.Id });
        }

        // Helpers
        private async Task LoadServicesDropDown(int? selectedId = null)
        {
            var services = await _context.Services.ToListAsync();

            var items = services
                .OrderBy(s => s.Name == "Other / Custom Request" ? 1 : 0)
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
