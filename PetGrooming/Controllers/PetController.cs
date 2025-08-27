using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using PetGroomingSystem.Models.ViewModels;

namespace PetGroomingSystem.Controllers
{
    public class PetsController : Controller
    {
        private readonly DB _context;

        public PetsController(DB context)
        {
            _context = context;
        }

        // GET: Pets
        public async Task<IActionResult> Index()
        {
            var pets = await _context.Pets
                .Include(p => p.Doctor)
                .OrderBy(p => p.Name)
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

            var pet = await _context.Pets
                .Include(p => p.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // GET: Pets/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new PetViewModel
            {
                Pet = new Pet { CreatedDate = DateTime.Now },
                Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name")
            };
            return View(viewModel);
        }

        // POST: Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(viewModel.Pet);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pet created successfully!";
                return RedirectToAction(nameof(Index));
            }

            viewModel.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name", viewModel.Pet.DoctorId);
            return View(viewModel);
        }

        // GET: Pets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
            {
                return NotFound();
            }

            var viewModel = new PetViewModel
            {
                Pet = pet,
                Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name", pet.DoctorId)
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewModel.Pet);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Pet updated successfully!";
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
                return RedirectToAction(nameof(Index));
            }

            viewModel.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name", viewModel.Pet.DoctorId);
            return View(viewModel);
        }

        // GET: Pets/AssignDoctor/5
        public async Task<IActionResult> AssignDoctor(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
            {
                return NotFound();
            }

            var viewModel = new AssignDoctorViewModel
            {
                PetId = pet.Id,
                PetName = pet.Name,
                DoctorId = pet.DoctorId ?? 0,
                Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name")
            };

            return View(viewModel);
        }

        // POST: Pets/AssignDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDoctor(AssignDoctorViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var pet = await _context.Pets.FindAsync(viewModel.PetId);
                if (pet != null)
                {
                    pet.DoctorId = viewModel.DoctorId;
                    _context.Update(pet);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Doctor assigned successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }

            viewModel.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name");
            return View(viewModel);
        }

        // GET: Pets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets
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
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pet deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.Id == id);
        }
    }
}