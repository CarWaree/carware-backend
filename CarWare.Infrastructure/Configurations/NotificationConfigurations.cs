using CarWare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Body)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ReferenceId)
            .HasMaxLength(100);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(50);

        builder.Property(x => x.DataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.SentAt)
            .IsRequired(false);

        builder.Property(x => x.ReadAt)
            .IsRequired(false);

        // ⚡ Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsRead);

        builder.HasIndex(x => new { x.UserId, x.IsRead });
    }
}