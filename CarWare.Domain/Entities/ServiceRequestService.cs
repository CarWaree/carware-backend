using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Domain.Entities
{
    public class ServiceRequestService
    {
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

        public int MaintenanceTypeId { get; set; }
        public MaintenanceType MaintenanceType { get; set; }

        public string Description { get; set; }
    }
}
