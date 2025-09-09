using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace PetGroomingSystem.ServiceRoleHelpers
{
    public static class ServiceRoleHelper
    {
        // Map of service type → required role
        private static readonly Dictionary<string, string> ServiceRoleMap = new()
        {
            { "Flea Treatment", "Doctor" },
            { "Vaccination", "Doctor" },
            { "Dental Care", "Doctor" },
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

