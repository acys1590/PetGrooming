namespace PetGroomingSystem.Models.ViewModels
{
    public class MemberDetailVM
    {
        public string Email { get; set; }          // Primary key
        public string Name { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string? UserPhoto { get; set; }
        public string? MemberPhoto { get; set; }
    }
}
