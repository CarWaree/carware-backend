using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Profile
{
    public class UploadImageDto
    {
        public IFormFile File { get; set; }
    }
}
