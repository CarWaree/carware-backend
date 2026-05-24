using CarWare.Application.Common;
using CarWare.Application.DTOs.Dashboard.Setup;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface ISetupService
    {
        Task<Result<SetupResponseDto>> CompleteSetupAsync(string adminUserId, SetupRequestDto dto);
    }
}