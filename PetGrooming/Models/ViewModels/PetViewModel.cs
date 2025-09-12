using Microsoft.AspNetCore.Mvc.Rendering;
using PetGrooming.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetGroomingSystem.Models.ViewModels
{
    public class PetViewModel
    {
        public required Appointment Pet { get; set; }

        public IEnumerable<SelectListItem> ServiceTypes { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Appointment Date is required")]
        [Display(Name = "Appointment Date")]
        [Column(TypeName = "datetime2")]
        public DateTime AppointmentDate { get; set; }
    }

   
    public class AssignDoctorViewModel
    {
        public int PetId { get; set; }
        public required string PetName { get; set; }
        public required string ServiceTypes { get; set; }

        // One dropdown
        public string SelectedPerson { get; set; } // e.g. "doctor_3" or "staff_5"
        public SelectList People { get; set; }
    }



}