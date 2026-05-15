using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWare.Domain.Entities
{
    [Table("ServiceType")]
    public class ServiceType : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        public ICollection<MaintenanceReminder> Reminders { get; set; }
            = new List<MaintenanceReminder>();

        public ICollection<CenterServiceCatalog> ProviderServices { get; set; }
            = new List<CenterServiceCatalog>();

        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}
