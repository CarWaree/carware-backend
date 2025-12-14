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

        public ICollection<ProviderServices> ProviderServices { get; set; } = new List<ProviderServices>();

        public ICollection<Appointment> Appointments { get; set; }
    }
}