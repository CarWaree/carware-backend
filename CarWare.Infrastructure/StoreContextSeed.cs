using CarWare.Domain.Entities;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarWare.Infrastructure
{
    public class StoreContextSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(assemblyFolder, "DataSeed", "egypt_car_brands_models.json");

        if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "{}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var carData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);

            if (carData == null || carData.Count == 0) return;

            var year = DateTime.UtcNow.Year;

            /* -----------------------------------------  
             * 1) Seed Brands  
             * ----------------------------------------- */
            foreach (var brandEntry in carData)
            {
                var brandName = brandEntry.Key?.Trim();
                if (string.IsNullOrWhiteSpace(brandName)) continue;

                if (!await context.brands.AnyAsync(b => b.Name == brandName))
                {
                    context.brands.Add(new Brand { Name = brandName });
                }
            }
            await context.SaveChangesAsync();

            /* -----------------------------------------  
             * 2) Seed Models  
             * ----------------------------------------- */
            var allBrands = await context.brands.ToListAsync();
            foreach (var brandEntry in carData)
            {
                var brand = allBrands.Find(b => b.Name == brandEntry.Key?.Trim());
                if (brand == null) continue;

                foreach (var modelNameRaw in brandEntry.Value)
                {
                    var modelName = modelNameRaw?.Trim();
                    if (string.IsNullOrWhiteSpace(modelName)) continue;

                    if (!await context.models.AnyAsync(m => m.Name == modelName && m.BrandId == brand.Id))
                    {
                        context.models.Add(new Model
                        {
                            Name = modelName,
                            BrandId = brand.Id
                        });
                    }
                }
            }
            await context.SaveChangesAsync();

            /* -----------------------------------------  
             * 3) Seed Vehicles  
             * ----------------------------------------- */
            var allModels = await context.models.ToListAsync();

            foreach (var brandEntry in carData)
            {
                var brand = allBrands.Find(b => b.Name == brandEntry.Key?.Trim());
                if (brand == null) continue;

                foreach (var modelNameRaw in brandEntry.Value)
                {
                    var modelName = modelNameRaw?.Trim();
                    var model = allModels.Find(m => m.Name == modelName && m.BrandId == brand.Id);
                    if (model == null) continue;

                    if (!await context.vehicles.AnyAsync(v => v.BrandId == brand.Id && v.ModelId == model.Id))
                    {
                        context.vehicles.Add(new Vehicle
                        {
                            Name = $"{brand.Name} {model.Name}", 
                            BrandId = brand.Id,
                            ModelId = model.Id,
                            Year = year
                        });
                    }
                }
            }
            await context.SaveChangesAsync();
        }
    }  
}