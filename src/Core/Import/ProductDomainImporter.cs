using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class ProductDomainImporter
{
    // Використовує наявний ImportResult<T> із лабораторної №3.
    public static ImportResult<Product> Convert(ImportResult<ProductDto> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var products = new List<Product>();
        var errors = new List<string>(result.Errors);
        int index = 0;

        foreach (ProductDto dto in result.Items)
        {
            index++;
            try
            {
                products.Add(Product.FromDto(dto));
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Доменна перевірка запису {index}: {ex.Message}");
            }
        }

        return new ImportResult<Product>(products.AsReadOnly(), errors.AsReadOnly());
    }
}
