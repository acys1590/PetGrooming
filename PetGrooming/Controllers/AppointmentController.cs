using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;

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
        public async Task<IActionResult> Create([Bind("OwnerName,PetName,PetType,PetBreed,Email,PhoneNumber,ServiceType,AppointmentDate,Notes")] Appointment appointment)
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

            // Validate appointment date is in the future
            if (appointment.AppointmentDate <= DateTime.Now)
            {
                ModelState.AddModelError("AppointmentDate", "Appointment date must be in the future.");
            }

            // NEW: If "Other", require Notes (>= 10 chars)
            if (string.Equals(appointment.ServiceType, "Other", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(appointment.Notes) || appointment.Notes!.Trim().Length < 10)
                {
                    ModelState.AddModelError("Notes",
                        "Please describe your custom request (at least 10 characters).");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["ServiceTypes"] = GetServiceTypeSelectList();
                ViewData["PetTypes"] = GetPetTypeSelectList();
                return View(appointment);
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
        // Grooming
        new SelectListItem { Value = "Basic Grooming", Text = "Basic Grooming (RM40)" },
        new SelectListItem { Value = "Full Grooming", Text = "Full Grooming (RM80)" },
        new SelectListItem { Value = "Bath Only", Text = "Bath Only (RM25)" },
        new SelectListItem { Value = "Nail Trim", Text = "Nail Trim (RM15)" },
        // Doctor
        new SelectListItem { Value = "Vet Consultation", Text = "Vet Consultation (RM60)" },
        new SelectListItem { Value = "General Health Check", Text = "General Health Check (RM80)" },
        new SelectListItem { Value = "Vaccination", Text = "Vaccination (RM70)" },
        new SelectListItem { Value = "Flea/Tick Treatment", Text = "Flea/Tick Treatment (RM50)" },
        new SelectListItem { Value = "Minor Wound Care", Text = "Minor Wound Care (RM90)" },
        new SelectListItem { Value = "Blood Test (Basic)", Text = "Blood Test (Basic) (RM120)" },
        new SelectListItem { Value = "Spay/Neuter", Text = "Spay/Neuter (from RM250)" },
        new SelectListItem { Value = "Dental Care", Text = "Dental Care (RM50)" },
        new SelectListItem { Value = "Other", Text = "Other / Custom Request" }
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