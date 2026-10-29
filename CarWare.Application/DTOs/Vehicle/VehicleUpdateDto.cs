
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Vehicle
{
    public class VehicleUpdateDTO
    {
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int BrandId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ModelId { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Color { get; set; } = string.Empty;
    }
}
