namespace PetGroomingSystem.Models.ViewModels
{
    public class HistoryVM
    {
        public int Id { get; set; }

        // From Appointment
        public string MemberName { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;

        // From Service
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Appointment details
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
    }
}
