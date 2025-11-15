using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Vehicle
{
    public class VehicleDTOs
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Model must be Required")]
        public string Model { get; set; }

        [Required(ErrorMessage = "Brand must be Required")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Year must be Required")]
        public int Year { get; set; }
        public string Color { get; set;}

        public string userId { get; set; } //in review
    }
}
