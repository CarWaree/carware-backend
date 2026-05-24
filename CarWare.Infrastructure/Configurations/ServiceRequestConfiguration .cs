using CarWare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarWare.Infrastructure.Data.Configurations
{
    public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
    {
        public void Configure(EntityTypeBuilder<ServiceRequest> builder)
        {
            builder.Property(x => x.TotalPrice)
                .HasPrecision(18, 2);

            builder.HasOne(sr => sr.Appointment)
                .WithMany()
                .HasForeignKey(sr => sr.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.User)
                .WithMany(u => u.ServiceRequests)
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Technician)
                .WithMany(u => u.HandledServiceRequests)
                .HasForeignKey(sr => sr.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.EstimatedCost)
                .HasPrecision(18, 2);
        }
    }
}