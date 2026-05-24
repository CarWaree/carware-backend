using CarWare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.HasOne(dt => dt.User)
               .WithMany(u => u.DeviceTokens)
               .HasForeignKey(dt => dt.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}