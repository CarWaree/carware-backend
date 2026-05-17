using CarWare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarWare.Infrastructure.Data.Configurations
{
    public class MaintenanceReminderConfiguration : IEntityTypeConfiguration<MaintenanceReminder>
    {
        public void Configure(EntityTypeBuilder<MaintenanceReminder> builder)
        {
            builder.HasOne(m => m.Vehicle)
                   .WithMany(v => v.maintenances)
                   .HasForeignKey(m => m.VehicleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Type)
                   .WithMany(t => t.Reminders)
                   .HasForeignKey(m => m.TypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}