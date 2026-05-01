using CarWare.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace CarWare.Application.Common.Security
{
    public interface IJwtTokenGenerator
    {
        Task<JwtSecurityToken> CreateToken(ApplicationUser user);
    }
}