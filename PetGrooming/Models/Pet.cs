using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace PetGroomingSystem.Models
{
    public class Pet
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Species { get; set; }

        [StringLength(50)]
        public string Breed { get; set; }

        public int Age { get; set; }

        [StringLength(20)]
        public string Gender { get; set; }

        [StringLength(100)]
        public string OwnerName { get; set; }

        [StringLength(15)]
        public string OwnerPhone { get; set; }

        [EmailAddress]
        public string OwnerEmail { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? LastGroomingDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        // Foreign Key
        public int? DoctorId { get; set; }

        // Navigation Property
        public virtual Doctor Doctor { get; set; }
    }
    namespace PetGrooming.Models
    {
        public class Pet
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Gender { get; set; }
            public string Species { get; set; }
            public string Breed { get; set; }
            public int Age { get; set; }
            public string OwnerName { get; set; }
            public string OwnerPhone { get; set; }
            public Doctor Doctor { get; set; }
        }

        public class Doctor
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
