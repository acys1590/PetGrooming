using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using PetGroomingSystem.Models.ViewModels;
using PetGroomingSystem.Services;
using PetGroomingSystem.ServiceHelpers;
using PetGroomingSystem.ServiceRoleHelpers;
using System.Net;
using System.Net.Mail;

namespace PetGroomingSystem.Controllers
{
    public class PetsController : Controller
    {
        private readonly DB _context;
        private readonly IEmailSender _emailSender;



        public PetsController(DB context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Pets
        public async Task<IActionResult> Index()
        {
            var pets = await _context.Appointments
                .Include(p => p.Doctor)
                .Include(p => p.Staff)
                .OrderBy(p => p.Id)
                .ToListAsync();
            return View(pets);
        }

        // GET: Pets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Appointments
                .Include(p => p.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // GET: Pets/Create
        public IActionResult Create()
        {
            var viewModel = new PetViewModel
            {
                Pet = new Appointment { AppointmentDate = DateTime.Now.AddDays(1) },

                ServiceTypes = _context.Services
                    .Where(s => s.IsActive)
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = $"{s.Name} - RM {s.Price}"
                    })
                    .ToList()


            };
            return View(viewModel);
        }



        // POST: Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // ✅ make sure appointment is in the future
                if (viewModel.Pet.AppointmentDate <= DateTime.Now)
                {
                    ModelState.AddModelError("Pet.AppointmentDate", "Appointment date must be in the future.");
                    return View(viewModel);
                }

                viewModel.ServiceTypes = _context.Services
               .Where(s => s.IsActive)
               .Select(s => new SelectListItem
               {
                   Value = s.Id.ToString(),
                   Text = $"{s.Name} - RM {s.Price}"
               })
               .ToList();

                _context.Add(viewModel.Pet);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pet created successfully!";
                return RedirectToAction(nameof(Index));
            }


            return View(viewModel);
        }


        // GET: Pets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Appointments.FindAsync(id);
            if (pet == null)
            {
                return NotFound();
            }

            var viewModel = new PetViewModel
            {
                Pet = pet,
                ServiceTypes = ServiceHelper.GetServiceTypeSelectList()
            };

            return View(viewModel);
        }

        // POST: Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetViewModel viewModel)
        {
            if (id != viewModel.Pet.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)  // ✅ fixed
            {
                try
                {
                    // ✅ validate appointment date is in the future
                    if (viewModel.Pet.AppointmentDate <= DateTime.Now)
                    {
                        ModelState.AddModelError("Pet.AppointmentDate", "Appointment date must be in the future.");
                        viewModel.ServiceTypes = ServiceHelper.GetServiceTypeSelectList();
                        return View(viewModel);
                    }

                    // ✅ only update Pet fields, preserve DoctorId, StaffId, IsApproved
                    var existingPet = await _context.Appointments.FindAsync(id);
                    if (existingPet == null) return NotFound();

                    existingPet.PetName = viewModel.Pet.PetName;
                    existingPet.PetType = viewModel.Pet.PetType;
                    existingPet.PetBreed = viewModel.Pet.PetBreed;
                    existingPet.Age = viewModel.Pet.Age;
                    existingPet.Gender = viewModel.Pet.Gender;
                    existingPet.ServiceType = viewModel.Pet.ServiceType;
                    existingPet.AppointmentDate = viewModel.Pet.AppointmentDate;
                    existingPet.OwnerName = viewModel.Pet.OwnerName;
                    existingPet.PhoneNumber = viewModel.Pet.PhoneNumber;
                    existingPet.Email = viewModel.Pet.Email;
                    existingPet.Notes = viewModel.Pet.Notes;

                    // ✅ DoctorId, StaffId, IsApproved stay unchanged

                    _context.Update(existingPet);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Pet updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetExists(viewModel.Pet.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // if validation fails, reload ServiceTypes and return PetViewModel
            viewModel.ServiceTypes = ServiceHelper.GetServiceTypeSelectList();
            return View(viewModel);
        }




        public async Task<IActionResult> AssignDoctor(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _context.Appointments.FindAsync(id);
            if (pet == null) return NotFound();

            string? requiredRole = ServiceRoleHelper.GetRequiredRole(pet.ServiceType);


            var doctors = await _context.Doctors.Where(d => d.IsActive).ToListAsync();
            var staffs = await _context.Staffs.Where(s => s.IsActive).ToListAsync();

            var people = new List<SelectListItem>();

            if (requiredRole == "Doctor" || requiredRole == null)
            {
                people.AddRange(doctors.Select(d => new SelectListItem
                {
                    Value = $"doctor_{d.Id}",
                    Text = $"Doctor - {d.Name}"
                }));
            }

            if (requiredRole == "Staff" || requiredRole == null)
            {
                people.AddRange(staffs.Select(s => new SelectListItem
                {
                    Value = $"staff_{s.Id}",
                    Text = $"Staff - {s.Name}"
                }));
            }

            var viewModel = new AssignDoctorViewModel
            {
                PetId = pet.Id,
                PetName = pet.PetName,
                ServiceTypes = pet.ServiceType,
                People = new SelectList(people, "Value", "Text"),
                SelectedPerson = pet.DoctorId.HasValue ? $"doctor_{pet.DoctorId}" :
                                  pet.StaffId.HasValue ? $"staff_{pet.StaffId}" : null
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDoctor(AssignDoctorViewModel viewModel)
        {
            var pet = await _context.Appointments.FindAsync(viewModel.PetId);
            if (pet == null) return NotFound();

            if (string.IsNullOrEmpty(viewModel.SelectedPerson))
            {
                ModelState.AddModelError("SelectedPerson", "Please select a doctor or staff.");

                // repopulate
                viewModel.People = await BuildPeopleSelectList();
                return View(viewModel);
            }

            // Reset first
            pet.DoctorId = null;
            pet.StaffId = null;

            if (viewModel.SelectedPerson.StartsWith("doctor_"))
            {
                pet.DoctorId = int.Parse(viewModel.SelectedPerson.Split('_')[1]);
            }
            else if (viewModel.SelectedPerson.StartsWith("staff_"))
            {
                pet.StaffId = int.Parse(viewModel.SelectedPerson.Split('_')[1]);
            }

            _context.Update(pet);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Assignment saved successfully!";
            return RedirectToAction(nameof(Index));
        }

        // helper to rebuild dropdown
        private async Task<SelectList> BuildPeopleSelectList()
        {
            var doctors = await _context.Doctors.Where(d => d.IsActive).ToListAsync();
            var staffs = await _context.Staffs.Where(s => s.IsActive).ToListAsync();

            var people = new List<SelectListItem>();

            people.AddRange(doctors.Select(d => new SelectListItem
            {
                Value = $"doctor_{d.Id}",
                Text = $"Doctor - {d.Name}"
            }));

            people.AddRange(staffs.Select(s => new SelectListItem
            {
                Value = $"staff_{s.Id}",
                Text = $"Staff - {s.Name}"
            }));

            return new SelectList(people, "Value", "Text");
        }


        // GET: Pets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Appointments
                .Include(p => p.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // POST: Pets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pet = await _context.Appointments.FindAsync(id);
            if (pet != null)
            {
                _context.Appointments.Remove(pet);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pet deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PetExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleApproval(int id, bool approved)
        {
            var pet = await _context.Appointments.FindAsync(id);
            if (pet == null)
            {
                return Json(new { success = false, message = "Pet not found." });
            }

            pet.IsApproved = approved;
            _context.Update(pet);
            await _context.SaveChangesAsync();

            string message;

            if (approved)
            {
                // ✅ When Approved
                await _emailSender.SendEmailAsync(
                    pet.Email,
                    "Appointment Approved",
                    $"Hello {pet.OwnerName},<br>Your appointment for {pet.PetName} has been <b>approved</b>."
                );

                message = "✅ Appointment approved. Email sent to customer.";
            }
            else
            {
                // ❌ When Unapproved
                await _emailSender.SendEmailAsync(
                    pet.Email,
                    "Appointment Unapproved",
                    $"Hello {pet.OwnerName},<br>Your appointment for {pet.PetName} has been <b>unapproved</b>. Please contact us for rescheduling."
                );

                message = "⚠️ Appointment unapproved. Email sent to customer.";
            }

            return Json(new { success = true, message });
        }


    }
}










