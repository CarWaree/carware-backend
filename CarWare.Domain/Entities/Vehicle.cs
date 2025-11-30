using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Vehicle : BaseEntity
{
    [Required]
    public string Name { get; set; }

    // FK to Brand
    public int BrandId { get; set; }
    public Brand Brand { get; set; }

    // FK to Model
    public int ModelId { get; set; }
    public Model Model { get; set; }

    // FK to user
    public string UserId { get; set; }
    public ApplicationUser user { get; set; }

    public ICollection<MaintenanceReminder> maintenances { get; set; }

    [Required]
    public int Year { get; set; }
    public string Color { get; set; }
}