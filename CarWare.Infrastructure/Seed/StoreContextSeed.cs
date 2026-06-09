using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Seed
{
    public class StoreContextSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.brands.AnyAsync() &&
                await context.models.AnyAsync())
            {
                return;
            }

            string assemblyFolder = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location)!;

            string filePath = Path.Combine(
                assemblyFolder,
                "DataSeed",
                "egypt_car_brands_models.json");

            if (!File.Exists(filePath))
            {
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);

            var carData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);

            if (carData == null || carData.Count == 0)
                return;

            // =========================
            // Seed Brands
            // =========================

            var existingBrandNames = await context.brands
                .AsNoTracking()
                .Select(b => b.Name)
                .ToListAsync();

            var brandLookup = existingBrandNames.ToHashSet(
                StringComparer.OrdinalIgnoreCase);

            foreach (var brandEntry in carData)
            {
                var brandName = brandEntry.Key?.Trim();

                if (string.IsNullOrWhiteSpace(brandName))
                    continue;

                if (!brandLookup.Contains(brandName))
                {
                    context.brands.Add(new Brand
                    {
                        Name = brandName
                    });
                }
            }

            await context.SaveChangesAsync();

            // =========================
            // Seed Models
            // =========================

            var allBrands = await context.brands
                .AsNoTracking()
                .ToListAsync();

            var existingModels = await context.models
                .AsNoTracking()
                .Select(m => new
                {
                    m.Name,
                    m.BrandId
                })
                .ToListAsync();

            var existingModelKeys = existingModels
                .Select(x => $"{x.BrandId}|{x.Name}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var brandEntry in carData)
            {
                var brandName = brandEntry.Key?.Trim();

                var brand = allBrands.FirstOrDefault(
                    b => b.Name.Equals(
                        brandName,
                        StringComparison.OrdinalIgnoreCase));

                if (brand == null)
                    continue;

                foreach (var modelNameRaw in brandEntry.Value)
                {
                    var modelName = modelNameRaw?.Trim();

                    if (string.IsNullOrWhiteSpace(modelName))
                        continue;

                    var key = $"{brand.Id}|{modelName}";

                    if (!existingModelKeys.Contains(key))
                    {
                        context.models.Add(new Model
                        {
                            Name = modelName,
                            BrandId = brand.Id
                        });

                        existingModelKeys.Add(key);
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}