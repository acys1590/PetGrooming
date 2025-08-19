using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetGroomingSystem.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; }

        [StringLength(15)]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        public DateTime JoinDate { get; set; }

        public bool IsActive { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        // Navigation Property
        public virtual ICollection<Pet> Pets { get; set; }

        public Doctor()
        {
            Pets = new HashSet<Pet>();
            IsActive = true;
            JoinDate = DateTime.Now;
        }
    }
}