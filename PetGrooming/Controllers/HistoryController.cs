using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetGrooming.Models;
using PetGroomingSystem.Models.ViewModels;

namespace PetGroomingSystem.Controllers
{
    public class HistoryController : Controller
    {
        private readonly DB db;
        public HistoryController(DB db) => this.db = db;

        // Appointment History (with search + paging)
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 5)
        {
            // ✅ Explicitly declare IQueryable<Appointment> to avoid CS0266
            IQueryable<Appointment> query = db.Appointments
                                              .Include(a => a.Service);

            // Search by OwnerName, Email, or Service.Name
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.OwnerName.Contains(search) ||
                    a.Email.Contains(search) ||
                    a.Service.Name.Contains(search));
            }

            // Total count for paging
            var totalItems = await query.CountAsync();

            // Paging + projection into HistoryVM
            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new HistoryVM
                {
                    Id = a.Id,
                    MemberName = a.OwnerName,
                    MemberEmail = a.Email,
                    ServiceName = a.Service.Name,
                    AppointmentDate = a.AppointmentDate,
                    Notes = a.Notes,
                    Price = a.Service.Price
                })
                .ToListAsync();

            // Wrap into List VM
            var model = new HistoryListVM
            {
                Appointments = appointments,
                Search = search,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(model);
        }

        // View details for a single appointment
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await db.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var model = new HistoryVM
            {
                Id = appointment.Id,
                MemberName = appointment.OwnerName,
                MemberEmail = appointment.Email,
                ServiceName = appointment.Service?.Name ?? "",
                Price = appointment.Service?.Price ?? 0,
                AppointmentDate = appointment.AppointmentDate,
                Notes = appointment.Notes
            };

            return View(model);
        }
    }
}
