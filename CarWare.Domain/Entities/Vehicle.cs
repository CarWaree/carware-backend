using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        [Required]
        public string Model { get; set; }
        [Required]
        public string Brand { get; set; }
        [Required]
        public int Year { get; set; }
        public string Color { get; set; }
        public string UserId { get; set; }
        public ApplicationUser user { get; set; }
    }
}