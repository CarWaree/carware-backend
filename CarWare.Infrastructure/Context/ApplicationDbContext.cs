using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarWare.Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public ApplicationDbContext() { }

        public DbSet<Vehicle> vehicles { get; set; }
        public DbSet<Maintenance> maintenances { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            builder.Entity<Vehicle>()
            .HasOne(c => c.user)           
            .WithMany(c => c.vehicles)                     
            .HasForeignKey(c => c.UserId)  
            .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Maintenance>()
            .HasOne(c => c.Vehicle)
            .WithMany(c => c.maintenances)
            .HasForeignKey(c => c.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Maintenance>()
            .Property(m => m.TypeOfMaintenance)
            .HasConversion<string>();
        }

        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=HP;Database=CarWare_DB;Trusted_Connection=True;Trust Server Certificate=True;");
            }
        }*/
    }
}