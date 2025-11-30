
namespace CarWare.Domain.Entities
{
    public class ProviderServices
    {
        public int ServiceCenterId { get; set; }
        public ServiceCenter ServiceCenter { get; set; }

        public int ServiceId { get; set; }
        public MaintenanceType Service { get; set; }
    }
}
