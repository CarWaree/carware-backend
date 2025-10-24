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
        Task<AuthModel> RegisterAsync(RegisterModel request);
    }
}
