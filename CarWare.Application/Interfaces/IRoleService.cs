using CarWare.Application.Common;
using CarWare.Application.DTOs.Role;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IRoleService
    {
        Task<Result<string>> CreateRoleAsync(CreateRoleDto dto);
        Task<Result<string>> AssignRoleAsync(AssignRoleDto dto);
        Task<Result<string>> RemoveRoleAsync(AssignRoleDto dto);
        Task<Result<List<string>>> GetRolesAsync();
        Task<Result<List<string>>> GetUserRolesAsync(string userId);
    }
}