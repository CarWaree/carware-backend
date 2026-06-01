using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Domain.Entities
{
    public class ServiceCenter : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        public string Location { get; set; }

        [Phone]
        public string Phone { get; set; }

        public TimeSpan WorkingFrom { get; set; }
        public TimeSpan WorkingTo { get; set; }

        public ICollection<Slot> Slots { get; set; } = new List<Slot>();

        public ICollection<ApplicationUser> Admins { get; set; } = new List<ApplicationUser>();

        public ICollection<ProviderServices> ProviderServices { get; set; } = new List<ProviderServices>();

        public ICollection<Appointment> Appointments { get; set; }
    }
}