
namespace CarWare.Domain.Entities
{
    public class ServiceOrderItem
    {
        public int ServiceRequestId { get; set; }
        public ServiceOrder ServiceRequest { get; set; }

        public int MaintenanceTypeId { get; set; }
        public MaintenanceType MaintenanceType { get; set; }

        public string Description { get; set; }
    }
}
