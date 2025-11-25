using CarWare.Domain.Entities;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarWare.Infrastructure
{
    public static class StoreContextSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.vehicles.AnyAsync())
                return; // Already seeded 

            var filePath = Path.Combine(AppContext.BaseDirectory, "egypt_car_brands_models.json");


            if (!File.Exists(filePath))
                return;

            var json = await File.ReadAllTextAsync(filePath);

            var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data is null)
                return;

            var vehicles = new List<Vehicle>();
            var year = DateTime.UtcNow.Year;

            foreach (var brand in data)
            {
                foreach (var model in brand.Value)
                {
                    vehicles.Add(new Vehicle
                    {
                        Brand = brand.Key,
                        Model = model,
                        Year = year,        
                        Color = null,       
                        UserId = null       
                    });
                }
            }

            await context.vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();
        }
    }
}
