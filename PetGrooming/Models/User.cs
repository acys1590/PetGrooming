using System.ComponentModel.DataAnnotations;

namespace PetGrooming.Models
{
    public class User
    {
        [Key]
        public string Email { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
