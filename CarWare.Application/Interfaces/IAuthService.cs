using CarWare.Application.DTOs.Auth;
using CarWare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IAuthService

    {
        Task<AuthDto> RegisterAsync(RegisterDto request);
        Task<AuthDto> LoginAsync(LoginDto loginDto);
        Task<AuthDto> LoginWithGoogleAsync(string googleToken);


    }
}
