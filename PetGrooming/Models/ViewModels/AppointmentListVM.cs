namespace PetGroomingSystem.Models.ViewModels
{
    using System.Collections.Generic;
    using PetGrooming.Models;

    public class AppointmentListVM
    {
        public string? Email { get; set; }
        public List<Appointment> Upcoming { get; set; } = new();
        public List<Appointment> History { get; set; } = new();
    }
}
