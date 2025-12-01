
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Vehicle
{
    public class VehicleCreateDTO
    {
        [Required]
        public int BrandId { get; set; }
        [Required]
        public int ModelId { get; set; }
        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }
        [Required]
        [StringLength(50)]
        public string Color { get; set; }
    }
}
