using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using System.Text.RegularExpressions;

namespace PetGroomingSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly DB _context;

        public object ServiceId { get; private set; }

        public AppointmentController(DB context)
        {
            DB _context = context;
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

        // GET: Appointment/Create
        public async Task<IActionResult> Create()
        {
            await LoadServicesDropDown();
            ViewData["PetTypes"] = GetPetTypeSelectList();
            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OwnerName,PetName,PetType,PetBreed,Age,Gender,Email,PhoneNumber,ServiceId,ServiceType,AppointmentDate,Notes")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                // (A) Mobile number: starts with 01 and 10–11 digits total
                if (!Regex.IsMatch(appointment.PhoneNumber ?? string.Empty, @"^01\d{8,9}$"))
                {
                    ModelState.AddModelError("PhoneNumber", "Please enter a valid mobile number (starts with 01, 10–11 digits).");
                    await LoadAndReturn(appointment);
                    return View(appointment);
                }

                // (B) Future date/time only
                if (appointment.AppointmentDate <= DateTime.Now)
                {
                    ModelState.AddModelError("AppointmentDate", "Appointment date must be in the future.");
                    await LoadAndReturn(appointment);
                    return View(appointment);
                }

                // (C) 15-minute slots only (:00, :15, :30, :45)
                if ((appointment.AppointmentDate.Minute % 15) != 0 || appointment.AppointmentDate.Second != 0)
                {
                    ModelState.AddModelError("AppointmentDate", "Appointments are only available in 15-minute slots (:00, :15, :30, :45).");
                    await LoadAndReturn(appointment);
                    return View(appointment);
                }

                // (D) Business hours only
                if (!IsWithinBusinessHours(appointment.AppointmentDate, out var hoursError))
                {
                    ModelState.AddModelError("AppointmentDate", hoursError);
                    await LoadAndReturn(appointment);
                    return View(appointment);
                }

                // (E) If service is "Other / Custom Request", require ≥ 10 chars in Notes
                var chosenService = await _context.Services.FindAsync(appointment.ServiceId);
                if (chosenService != null && chosenService.Name == "Other / Custom Request")
                {
                    if (string.IsNullOrWhiteSpace(appointment.Notes) || appointment.Notes.Trim().Length < 10)
                    {
                        ModelState.AddModelError("Notes", "Please describe your custom request (at least 10 characters).");
                        await LoadAndReturn(appointment);
                        return View(appointment);
                    }
                }

                appointment.CreatedDate = DateTime.Now;
                _context.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment booked successfully!";
                return RedirectToAction(nameof(Confirmation), new { id = appointment.Id });
            }

            await LoadAndReturn(appointment);
            return View(appointment);
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

        // -------------------------
        // Helpers
        // -------------------------

        private async Task LoadAndReturn(Appointment appointment)
        {
            await LoadServicesDropDown(appointment?.ServiceId);
            ViewData["PetTypes"] = GetPetTypeSelectList();
        }

        private async Task LoadServicesDropDown(int? selectedId = null)
        {
            var services = await _context.Services
                .AsNoTracking()
                .Where(s => s.IsActive)
                .ToListAsync();

            var items = services
                .OrderBy(s => s.Name == "Other / Custom Request" ? 1 : 0) // "Other" last
                .ThenBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Name} (RM{s.Price:0.00})"
                })
                .ToList();

            ViewBag.ServiceId = new SelectList(items, "Value", "Text", selectedId?.ToString());
        }

        private static bool IsWithinBusinessHours(DateTime localDateTime, out string error)
        {
            // Business hours (local):
            // Mon–Fri: 08:00–18:00
            // Sat:     09:00–16:00
            // Sun:     Closed
            error = string.Empty;

            var dow = localDateTime.DayOfWeek;
            var t = localDateTime.TimeOfDay;

            TimeSpan open, close;

            switch (dow)
            {
                case DayOfWeek.Saturday:
                    open = new TimeSpan(9, 0, 0);
                    close = new TimeSpan(16, 0, 0);
                    break;
                case DayOfWeek.Sunday:
                    error = "We’re closed on Sundays. Please pick Monday–Saturday during business hours.";
                    return false;
                default: // Mon–Fri
                    open = new TimeSpan(8, 0, 0);
                    close = new TimeSpan(18, 0, 0);
                    break;
            }

            // Start inclusive, end exclusive
            if (t < open || t >= close)
            {
                error = (dow == DayOfWeek.Saturday)
                    ? "Saturday hours are 9:00 AM – 4:00 PM. Please pick a time within that range."
                    : "Weekday hours are 8:00 AM – 6:00 PM (Mon–Fri). Please pick a time within that range.";
                return false;
            }

            return true;
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
