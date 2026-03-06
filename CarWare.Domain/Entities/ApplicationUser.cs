using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; }
        [Required, StringLength(100)]
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public string ProfileImageUrl { get; set; }

        public ICollection<Vehicle> vehicles { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public string PendingEmail { get; set; }
    }
}