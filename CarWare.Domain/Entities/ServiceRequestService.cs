
namespace CarWare.Domain.Entities
{
    public class ServiceRequestService : BaseEntity
    {
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

        public int MaintenanceTypeId { get; set; }
        public MaintenanceType MaintenanceType { get; set; }

        public string Description { get; set; }
    }
}
