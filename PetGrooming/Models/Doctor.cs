//using System.ComponentModel.DataAnnotations;

//namespace PetGroomingSystem.Models
//{
//    public class Doctor
//    {
//        public int Id { get; set; }

//        [Required]
//        [StringLength(100)]
//        public string Name { get; set; } = string.Empty;

//        [StringLength(100)]
//        public string? Specialization { get; set; }

//        [Phone]
//        [StringLength(20)]
//        public string? Phone { get; set; }

//        [EmailAddress]
//        [StringLength(100)]
//        public string? Email { get; set; }

//        [StringLength(200)]
//        public string? Address { get; set; }

//        public DateTime JoinDate { get; set; }

//        public bool IsActive { get; set; }

//        [StringLength(500)]
//        public string? Notes { get; set; }

//        // Navigation property
//        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
//    }
//}