namespace PetGrooming.Models
{
    public class User
    {
        public string Hash { get; internal set; }
        public string Email { get; internal set; }
        public string Role { get; internal set; }
    }
}