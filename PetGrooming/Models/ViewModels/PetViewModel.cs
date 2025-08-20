using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetGroomingSystem.Models.ViewModels
{
    public class PetViewModel
    {
        public required Pet Pet { get; set; }
        public required SelectList Doctors { get; set; }
    }

    public class AssignDoctorViewModel
    {
        public int PetId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public required string PetName { get; set; }
        public required SelectList Doctors { get; set; }
    }
}