using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarWare.Infrastructure.Services
{
    public class VehicleLookupFileService
    {
        private readonly string _filePath;

        public VehicleLookupFileService(string filePath)
        {
            _filePath = filePath;
        }

        // Read JSON file into Dictionary<string, List<string>>
        public async Task<Dictionary<string, List<string>>> ReadAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new Dictionary<string, List<string>>();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json)
                   ?? new Dictionary<string, List<string>>();
        }

        // Write dictionary back to JSON file
        public async Task WriteAsync(Dictionary<string, List<string>> data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        // Add a new Brand
        public async Task AddBrandAsync(string brand)
        {
            var data = await ReadAsync();

            if (!data.ContainsKey(brand))
            {
                data[brand] = new List<string>();
                await WriteAsync(data);
            }
        }

        // Add a new Model to a Brand
        public async Task AddModelAsync(string brand, string model)
        {
            var data = await ReadAsync();

            if (!data.ContainsKey(brand))
            {
                data[brand] = new List<string>();
            }

            if (!data[brand].Contains(model))
            {
                data[brand].Add(model);
                await WriteAsync(data);
            }
        }

        // Optional: Remove a Model from a Brand
        public async Task RemoveModelAsync(string brand, string model)
        {
            var data = await ReadAsync();

            if (data.ContainsKey(brand) && data[brand].Contains(model))
            {
                data[brand].Remove(model);
                await WriteAsync(data);
            }
        }

        // Optional: Remove a Brand completely
        public async Task RemoveBrandAsync(string brand)
        {
            var data = await ReadAsync();

            if (data.ContainsKey(brand))
            {
                data.Remove(brand);
                await WriteAsync(data);
            }
        }
    }
}
