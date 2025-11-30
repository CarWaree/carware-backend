using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Domain.Entities
{
    public class MaintenanceType : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        public ICollection<MaintenanceReminder> Reminders { get; set; } 
            = new List<MaintenanceReminder>();

        public ICollection<ProviderServices> ProviderServices { get; set; } 
            = new List<ProviderServices>();
    }
}
