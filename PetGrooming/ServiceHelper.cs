using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetGroomingSystem.ServiceHelpers
{
    public static class ServiceHelper
    {
        public static SelectList GetServiceTypeSelectList()
        {
            var serviceTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Basic Grooming", Text = "Basic Grooming ($40)" },
                new SelectListItem { Value = "Full Grooming", Text = "Full Grooming ($80)" },
                new SelectListItem { Value = "Bath Only", Text = "Bath Only ($25)" },
                new SelectListItem { Value = "Nail Trim", Text = "Nail Trim ($15)" },
                new SelectListItem { Value = "Flea Treatment", Text = "Flea Treatment ($35)" },
                new SelectListItem { Value = "Dental Care", Text = "Dental Care ($50)" }
            };

            return new SelectList(serviceTypes, "Value", "Text");
        }

       
    }
}
