using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class CarImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly Dictionary<string, string> _imageIndex;

    public CarImageService(IWebHostEnvironment env)
    {
        _env = env;

        var folder = Path.Combine(_env.WebRootPath, "cars");

        if (!Directory.Exists(folder))
        {
            _imageIndex = new Dictionary<string, string>();
            return;
        }

        _imageIndex = Directory.GetFiles(folder)
            .Select(file =>
            {
                var name = Path.GetFileNameWithoutExtension(file);

                var parts = name.Split('-', 2, StringSplitOptions.TrimEntries);

                if (parts.Length < 2)
                    return null;

                var key = $"{Normalize(parts[0])}-{Normalize(parts[1])}";

                return new
                {
                    Key = key,
                    Path = $"/cars/{Path.GetFileName(file)}"
                };
            })
            .Where(x => x != null)
            .GroupBy(x => x.Key) // ✅ حل مشكلة التكرار
            .ToDictionary(g => g.Key, g => g.First().Path);
    }

    public string GetImageUrl(string brand, string model)
    {
        var key = $"{Normalize(brand)}-{Normalize(model)}";

        return _imageIndex.TryGetValue(key, out var path)
            ? path
            : "/cars/default.png";
    }

    private string Normalize(string input)
    {
        input = input.ToLower().Trim();
        input = Regex.Replace(input, @"[^a-z0-9]+", "-");
        input = Regex.Replace(input, "-{2,}", "-");
        return input.Trim('-');
    }
}