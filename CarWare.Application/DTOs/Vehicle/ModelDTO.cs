using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Vehicle
{
    public class ModelDTO
    {
        public int id { get; set; }
        [Required]
        public string Name { get; set; }
        public string BrandName { get; set; }
    }
}