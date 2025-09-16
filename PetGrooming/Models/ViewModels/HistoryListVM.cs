using System.Collections.Generic;

namespace PetGroomingSystem.Models.ViewModels
{
    public class HistoryListVM
    {
        public List<HistoryVM> Appointments { get; set; } = new List<HistoryVM>();

        // For search + paging
        public string? Search { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        // Optional: calculate total pages
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / PageSize);
    }
}
