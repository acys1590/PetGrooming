using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;

namespace PetGroomingSystem.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly DB _context;

        public DoctorsController(DB context)
        {
            _context = context;
        }

        // GET: Doctors
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Pets)
                .OrderBy(d => d.Name)
                .ToListAsync();
            return View(doctors);
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Pets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctors/Create
        public IActionResult Create()
        {
            var doctor = new Doctor();
            return View(doctor);
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                doctor.JoinDate = DateTime.Now;
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Doctor created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Doctor updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Pets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Doctor deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Doctors/ToggleStatus/5
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Pets) // ✅ include pets
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor != null)
            {
                doctor.IsActive = !doctor.IsActive;

                // ✅ If doctor becomes inactive, unassign pets
                if (!doctor.IsActive && doctor.Pets.Any())
                {
                    foreach (var pet in doctor.Pets)
                    {
                        pet.DoctorId = null;
                    }
                }

                _context.Update(doctor);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Doctor status changed to {(doctor.IsActive ? "Active" : "Inactive")}!";
            }

            return RedirectToAction(nameof(Index));
        }


        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }
    }
}