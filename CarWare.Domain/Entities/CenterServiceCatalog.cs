
namespace CarWare.Domain.Entities
{
    public class CenterServiceCatalog
    {
        public int ServiceCenterId { get; set; }
        public ServiceCenter ServiceCenter { get; set; }

        public int ServiceId { get; set; }
        public ServiceType ServiceType { get; set; }
    }
}
