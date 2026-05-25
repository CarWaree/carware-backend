using CarWare.Application.Common;
using CarWare.Application.DTOs.Dashboard.Profile;
using CarWare.Application.DTOs.Profile;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IProfileService
    {
        Task<Result<EditProfileResponseDto> >GetProfileAsync(string userId);
        Task<Result<string>> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<Result<string>> UploadImageAsync(string userId, IFormFile file);
        Task<Result<CenterProfileDto>> GetCenterProfileAsync(string UserId);
    }
}