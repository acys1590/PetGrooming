
namespace PetGrooming.Controllers
{
    public class UpdateProfileVM
    {
        public string Name { get; internal set; }
        public string PhotoURL { get; internal set; }
        public string Email { get; internal set; }
        public IFormFile Photo { get; internal set; }
    }
}