using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Model : BaseEntity
{
    [Required]
    public string Name { get; set; }
    public int BrandId { get; set; }
    public Brand Brand { get; set; }
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}