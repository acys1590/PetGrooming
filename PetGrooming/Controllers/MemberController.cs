using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using PetGroomingSystem.Models.ViewModels;

namespace PetGroomingSystem.Controllers
{
    public class MemberController : Controller
    {
        private readonly DB db;
        private readonly IWebHostEnvironment env;

        public MemberController(DB db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }

        // Index: List with Search + Paging
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 5)
        {
            var query = db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
            }

            int total = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var members = new List<MemberDetailVM>();
            foreach (var user in users)
            {
                var member = await db.Members.FirstOrDefaultAsync(m => m.Email == user.Email);
                members.Add(new MemberDetailVM
                {
                    Email = user.Email,
                    Name = user.Name,
                    Address = member?.Address,
                    DateOfBirth = member?.DateOfBirth,
                    Age = member?.Age,
                    UserPhoto = user.PhotoPath,
                    MemberPhoto = member?.PhotoURL
                });
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.TotalMembers = total;

            return View(members);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View(new MemberDetailVM());
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberDetailVM vm, IFormFile? Photo)
        {
            if (ModelState.IsValid)
            {
                string? fileName = null;
                if (Photo != null && Photo.Length > 0)
                {
                    var folder = Path.Combine(env.WebRootPath, "photos");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    fileName = Guid.NewGuid() + Path.GetExtension(Photo.FileName);
                    var path = Path.Combine(folder, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await Photo.CopyToAsync(stream);
                }

                var user = new User
                {
                    Email = vm.Email,
                    Name = vm.Name,
                    PhotoPath = fileName
                };
                db.Users.Add(user);

                var member = new Member
                {
                    Email = vm.Email,
                    Name = vm.Name,
                    Address = vm.Address,
                    DateOfBirth = vm.DateOfBirth,
                    Age = vm.Age,
                    PhotoURL = fileName
                };
                db.Members.Add(member);

                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(string email)
        {
            if (string.IsNullOrEmpty(email)) return NotFound();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            var member = await db.Members.FirstOrDefaultAsync(m => m.Email == email);

            if (user == null && member == null) return NotFound();

            var vm = new MemberDetailVM
            {
                Email = user?.Email ?? member?.Email,
                Name = user?.Name ?? member?.Name,
                Address = member?.Address,
                DateOfBirth = member?.DateOfBirth,
                Age = member?.Age,
                UserPhoto = user?.PhotoPath,
                MemberPhoto = member?.PhotoURL
            };

            return View(vm);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MemberDetailVM vm, IFormFile? Photo)
        {
            if (ModelState.IsValid)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
                var member = await db.Members.FirstOrDefaultAsync(m => m.Email == vm.Email);

                if (user == null || member == null) return NotFound();

                if (Photo != null && Photo.Length > 0)
                {
                    var folder = Path.Combine(env.WebRootPath, "photos");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    var fileName = Guid.NewGuid() + Path.GetExtension(Photo.FileName);
                    var path = Path.Combine(folder, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await Photo.CopyToAsync(stream);

                    user.PhotoPath = fileName;
                    member.PhotoURL = fileName;
                }

                user.Name = vm.Name;
                member.Name = vm.Name;
                member.Address = vm.Address;
                member.DateOfBirth = vm.DateOfBirth;
                member.Age = vm.Age;

                db.Update(user);
                db.Update(member);
                await db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(string email)
        {
            if (string.IsNullOrEmpty(email)) return NotFound();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            var member = await db.Members.FirstOrDefaultAsync(m => m.Email == email);

            if (user == null && member == null) return NotFound();

            var vm = new MemberDetailVM
            {
                Email = user?.Email ?? member?.Email,
                Name = user?.Name ?? member?.Name,
                Address = member?.Address,
                DateOfBirth = member?.DateOfBirth,
                Age = member?.Age,
                UserPhoto = user?.PhotoPath,
                MemberPhoto = member?.PhotoURL
            };

            return View(vm);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string email)
        {
            var user = await db.Users.FindAsync(email);
            var member = await db.Members.FindAsync(email);

            if (user != null) db.Users.Remove(user);
            if (member != null) db.Members.Remove(member);

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
