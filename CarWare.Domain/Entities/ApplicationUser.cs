using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CarWare.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        public string LastName { get; set; }

        public ICollection<Vehicle> vehicles { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
    }
}
