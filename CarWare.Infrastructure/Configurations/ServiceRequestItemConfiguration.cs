using CarWare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarWare.Infrastructure.Data.Configurations
{
    public class ServiceRequestItemConfiguration : IEntityTypeConfiguration<ServiceRequestItem>
    {
        public void Configure(EntityTypeBuilder<ServiceRequestItem> builder)
        {
            builder.HasKey(x => new { x.ServiceRequestId, x.MaintenanceTypeId });

            builder.HasOne(x => x.ServiceRequest)
                   .WithMany(sr => sr.ServiceRequestServices)
                   .HasForeignKey(x => x.ServiceRequestId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.MaintenanceType)
                   .WithMany()
                   .HasForeignKey(x => x.MaintenanceTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}