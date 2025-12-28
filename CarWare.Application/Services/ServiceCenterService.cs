using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Provider_Center;
using CarWare.Application.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace CarWare.Application.Services
{
    public class ServiceCenterService : IServiceCenterService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ServiceCenterService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ServiceCenterDto>>> GetAllAsync()
        {
            var entities = await _context.ServiceCenters.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<ServiceCenterDto>>(entities);
            return Result<IEnumerable<ServiceCenterDto>>.Ok(dtos);
        }

        public async Task<Result<IEnumerable<ServiceCenterDto>>> GetByServiceTypeAsync(int serviceTypeId)
        {
            var entities = await _context.ServiceCenters
                .Include(sc => sc.ProviderServices)
                .Where(sc => sc.ProviderServices.Any(ps => ps.ServiceId == serviceTypeId))
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<ServiceCenterDto>>(entities);
            return Result<IEnumerable<ServiceCenterDto>>.Ok(dtos);
        }
    }
}