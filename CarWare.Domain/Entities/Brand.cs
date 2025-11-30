using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Brand : BaseEntity
{
    [Required]
    public string Name { get; set; }
    public ICollection<Model> Models { get; set; } = new List<Model>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}