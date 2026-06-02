using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Profile
{
    public class UpdateProfileDto
    {
        [Required]
        public string FullName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}