using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;

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
                // Validate appointment date is in the future
                if (appointment.AppointmentDate <= DateTime.Now)
                {
                    ModelState.AddModelError("AppointmentDate", "Appointment date must be in the future.");
                    await LoadServicesDropDown(appointment.ServiceId);
                    ViewData["PetTypes"] = GetPetTypeSelectList();
                    return View(appointment);
                }

                // If service is "Other", require Notes
                var chosenService = await _context.Services.FindAsync(appointment.ServiceId);
                if (chosenService != null && chosenService.Name == "Other / Custom Request")
                {
                    if (string.IsNullOrWhiteSpace(appointment.Notes) || appointment.Notes.Trim().Length < 10)
                    {
                        ModelState.AddModelError("Notes", "Please describe your custom request (at least 10 characters).");
                        await LoadServicesDropDown(appointment.ServiceId);
                        ViewData["PetTypes"] = GetPetTypeSelectList();
                        return View(appointment);
                    }
                }

                appointment.CreatedDate = DateTime.Now;
                _context.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment booked successfully!";
                return RedirectToAction("Index", "Payment", new { serviceId = appointment.ServiceId });
            }

            await LoadServicesDropDown(appointment.ServiceId);
        }
                
            await LoadServicesDropDown(appointment.ServiceId);
            ViewData["PetTypes"] = GetPetTypeSelectList();
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

        private async Task LoadServicesDropDown(int? selectedId = null)
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .ToListAsync();

            var items = services
                .OrderBy(s => s.Name == "Other / Custom Request" ? 1 : 0) // put "Other" last
                .ThenBy(s => s.Name)                                      // sort the rest alphabetically
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
