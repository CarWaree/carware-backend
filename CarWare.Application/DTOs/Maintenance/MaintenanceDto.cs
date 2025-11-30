using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Maintenance
{
    public class MaintenanceDto
    {
        public int? MaintenanceId { get; set; }   
        [Required]  

        public string TypeOfMaintenance { get; set; }

        [Required]
        public DateTime NotificationDate { get; set; }

        [Required]
        public DateTime NextDueDate { get; set; }

        public DateTime? CreatedAt { get; set; }  
        public DateTime? UpdatedAt { get; set; }  

        [Required]
        public int VehicleId { get; set; }
    }
}
