using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Domain.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public int ServiceCenterId { get; set; }
        public ServiceCenter ServiceCenter { get; set; }

        public decimal TotalPrice { get; set; }
        public string Status { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<ServiceRequestService> ServiceRequestServices { get; set; }
    }
}
