using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Vehicle
{
    public class BrandDTO
    {
        public int id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}