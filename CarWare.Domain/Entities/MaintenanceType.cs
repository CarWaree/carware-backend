using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWare.Domain.Entities
{

    [Table("maintenanceTypes")]
    public class MaintenanceType : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        public ICollection<MaintenanceReminder> Reminders { get; set; } 
            = new List<MaintenanceReminder>();

        public ICollection<ProviderServices> ProviderServices { get; set; } 
            = new List<ProviderServices>();

        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}
