using Microsoft.AspNetCore.Identity;

namespace PrescribingSystem.Data
{
    public class ApplicationUser:IdentityUser
    { 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string HealthCouncilRegistrationNumber { get; set; }
        public string IdentityNumber { get; set; }
    }
}
