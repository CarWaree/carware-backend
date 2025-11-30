using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Maintenance
{
    public class ProviderServiceDto
    {
        public int ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; }

        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
    }
}
