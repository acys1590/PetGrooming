namespace PetGrooming.Controllers
{
    public class LoginVM
    {
        public bool RememberMe { get; internal set; }
        public string? Password { get; internal set; }
        public object?[]? Email { get; internal set; }
    }
}