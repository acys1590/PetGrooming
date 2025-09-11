using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace PetGroomingSystem.ServiceRoleHelpers
{
    public static class ServiceRoleHelper
    {
        // Map of service type → required role
        private static readonly Dictionary<string, string> ServiceRoleMap = new()
        {
            { "Flea/Tick Treatment", "Doctor" },
            { "Vaccination", "Doctor" },
            { "Dental Care", "Doctor" },
            { "Vet Consultation", "Doctor" },
            { "General Health Check", "Doctor" },
            { "Minor Wound Care", "Doctor" },
            { "Blood Test (Basic)", "Doctor" },
            { "Spay/Neuter", "Doctor" },
            { "Dental Care", "Doctor" },
            { "Other", "Doctor" },

            { "Basic Grooming", "Staff" },
            { "Full Grooming", "Staff" },
            { "Bath Only", "Staff" },
            { "Nail Trim", "Staff" }
        };

        public static string? GetRequiredRole(string serviceType)
        {
            if (string.IsNullOrWhiteSpace(serviceType)) return null;

            return ServiceRoleMap.TryGetValue(serviceType, out var role) ? role : null;
        }
    }
}

