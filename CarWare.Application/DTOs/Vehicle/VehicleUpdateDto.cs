
namespace CarWare.Application.DTOs.Vehicle
{
    public class VehicleUpdateDTO
    {
        public int Id { get; set; }
        public int? BrandId { get; set; }
        public int? ModelId { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
    }
}