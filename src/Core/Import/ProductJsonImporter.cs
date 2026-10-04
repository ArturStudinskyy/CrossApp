using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<ProductDto> deserialized =
                JsonSerializer.Deserialize<List<ProductDto>>(
                    json,
                    options
                ) ?? [];

            var items = new List<ProductDto>();

            for (int i = 0; i < deserialized.Count; i++)
            {
                ProductDto product = deserialized[i];

                if (string.IsNullOrWhiteSpace(product.Id) ||
                    string.IsNullOrWhiteSpace(product.Sku) ||
                    string.IsNullOrWhiteSpace(product.Name) ||
                    string.IsNullOrWhiteSpace(product.Unit) ||
                    product.Quantity < 0)
                {
                    errors.Add(
                        $"JSON елемент {i + 1}: некоректні дані товару"
                    );

                    continue;
                }

                items.Add(product);
            }

            return new ImportResult<ProductDto>(items, errors);
        }
        catch (JsonException ex)
        {
            errors.Add($"Помилка JSON: {ex.Message}");

            return new ImportResult<ProductDto>(
                [],
                errors
            );
        }
    }
}