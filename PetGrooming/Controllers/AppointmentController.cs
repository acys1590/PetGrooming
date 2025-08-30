using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using PetGroomingSystem.Models;

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
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
            return View(appointments);
        }

        // GET: Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointment/Create
        public IActionResult Create()
        {
            ViewData["ServiceTypes"] = GetServiceTypeSelectList();
            ViewData["PetTypes"] = GetPetTypeSelectList();
            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OwnerName,PetName,PetType,Email,PhoneNumber,ServiceType,AppointmentDate,Notes")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                // Validate appointment date is in the future
                if (appointment.AppointmentDate <= DateTime.Now)
                {
                    ModelState.AddModelError("AppointmentDate", "Appointment date must be in the future.");
                    ViewData["ServiceTypes"] = GetServiceTypeSelectList();
                    ViewData["PetTypes"] = GetPetTypeSelectList();
                    return View(appointment);
                }

                appointment.CreatedDate = DateTime.Now;
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment booked successfully!";
                return RedirectToAction(nameof(Confirmation), new { id = appointment.Id });
            }

            ViewData["ServiceTypes"] = GetServiceTypeSelectList();
            ViewData["PetTypes"] = GetPetTypeSelectList();
            return View(appointment);
        }

        // GET: Appointment/Confirmation/5
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        private SelectList GetServiceTypeSelectList()
        {
            var serviceTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Basic Grooming", Text = "Basic Grooming ($40)" },
                new SelectListItem { Value = "Full Grooming", Text = "Full Grooming ($80)" },
                new SelectListItem { Value = "Bath Only", Text = "Bath Only ($25)" },
                new SelectListItem { Value = "Nail Trim", Text = "Nail Trim ($15)" },
                new SelectListItem { Value = "Flea Treatment", Text = "Flea Treatment ($35)" },
                new SelectListItem { Value = "Dental Care", Text = "Dental Care ($50)" }
            };
            return new SelectList(serviceTypes, "Value", "Text");
        }

        private SelectList GetPetTypeSelectList()
        {
            var petTypes = new List<SelectListItem>
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