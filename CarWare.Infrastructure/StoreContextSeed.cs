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
            Console.WriteLine("Starting vehicle seeding...");

            if (await context.vehicles.AnyAsync())
            {
                Console.WriteLine("Vehicles table already has data. Skipping seeding.");
                return;
            }

            var filePath = "../CarWare.Infrastructure/DataSeed/egypt_car_brands_models.json";
            Console.WriteLine($"Looking for JSON file at: {filePath}");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("JSON file not found!");
                return;
            }

            Console.WriteLine("JSON file found, reading data...");

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data == null)
            {
                Console.WriteLine("Failed to deserialize JSON data.");
                return;
            }

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

            Console.WriteLine($"Number of vehicles to seed: {vehicles.Count}");

            await context.vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();

            Console.WriteLine("Vehicle seeding completed!");
        }
    }
}