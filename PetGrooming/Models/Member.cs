using System.ComponentModel.DataAnnotations;

namespace PetGrooming
{
    public class Member
    {
        [Key] // Email 是主键
        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string PhotoURL { get; set; } = string.Empty;
    }
}
