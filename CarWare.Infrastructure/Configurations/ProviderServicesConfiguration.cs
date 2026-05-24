using CarWare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarWare.Infrastructure.Data.Configurations
{
    public class ProviderServicesConfiguration : IEntityTypeConfiguration<ProviderServices>
    {
        public void Configure(EntityTypeBuilder<ProviderServices> builder)
        {
            //Provider Service [M:M]
            builder.HasKey(ps => new { ps.ServiceCenterId, ps.ServiceId });

            builder.HasOne(ps => ps.ServiceCenter)
                   .WithMany(sc => sc.ProviderServices)
                   .HasForeignKey(ps => ps.ServiceCenterId);

            builder.HasOne(ps => ps.Service)
                   .WithMany(s => s.ProviderServices)
                   .HasForeignKey(ps => ps.ServiceId);
        }
    }
}