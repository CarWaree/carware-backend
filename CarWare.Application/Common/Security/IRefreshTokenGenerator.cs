using CarWare.Domain.Entities;

namespace CarWare.Application.Common.Security
{
    public interface IRefreshTokenGenerator
    {
        RefreshToken Generate();
    }
}