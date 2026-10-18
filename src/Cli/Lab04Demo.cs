using Core.Domain;
using Core.Dto;
using Core.Import;

internal static class Lab04Demo
{
    public static void Run()
    {
        Console.WriteLine("=== Лабораторна №4: доменна модель складу ===");

        Console.WriteLine("\n1. Успішні операції та мапінг DTO");
        Product product = Product.Create("P-001", "sku-001", "Молоко", "шт", 100, "Тест");
        Console.WriteLine($"Спочатку: {product}");
        product.RegisterArrival(50);
        product.Issue(30);
        Console.WriteLine($"Після +50 та -30: {product}");
        ProductDto dto = product.ToDto();
        Product restored = Product.FromDto(dto);
        Console.WriteLine($"Із DTO: {restored}; Note = {restored.Note}");

        Console.WriteLine("\n2. Порушення інваріантів");
        TryDo("Перевитрата", () => product.Issue(1000));
        TryDo("Порожній SKU", () => Product.Create("P-002", " ", "Сир", "шт", 10));
        TryDo("Від'ємна кількість", () => Product.Create("P-003", "SKU-003", "Хліб", "шт", -5));
        TryDo("Нульовий прихід", () => product.RegisterArrival(0));
        Console.WriteLine($"Залишок після помилок: {product.Quantity} (очікується 120)");

        Console.WriteLine("\n3. Додаткове №1: перевірка результату імпорту");
        var imported = new ImportResult<ProductDto>(
            new List<ProductDto>
            {
                new("P-010", "SKU-010", "Цукор", "кг", 100),
                new("P-011", " ", "Сіль", "кг", 30),
                new("P-012", "SKU-012", "Борошно", "кг", -10)
            },
            new List<string> { "Приклад помилки CSV: пошкоджений рядок" }
        );
        ImportResult<Product> validated = ProductDomainImporter.Convert(imported);
        Console.WriteLine($"Коректні: {validated.Items.Count}; помилки: {validated.Errors.Count}");
        foreach (string error in validated.Errors)
            Console.WriteLine($"! {error}");

        string csvPath = Path.Combine("data", "correct.csv");
        if (File.Exists(csvPath))
        {
            var csvResult = ProductCsvImporter.Load(csvPath);
            var domainResult = ProductDomainImporter.Convert(csvResult);
            Console.WriteLine($"Із {csvPath}: {domainResult.Items.Count} сутностей, " +
                              $"{domainResult.Errors.Count} помилок");
        }

        Console.WriteLine("\n4. Додаткове №2: спільний інваріант товару і партії");
        Product stockProduct = Product.Create("P-020", "SKU-020", "Йогурт", "шт", 120);
        StockBatch batch = StockBatch.Create("B-001", "P-020", new DateTime(2026, 10, 1), 40);
        WarehouseStockService.IssueFromBatch(stockProduct, batch, 20);
        TryDo("Перевитрата партії", () =>
            WarehouseStockService.IssueFromBatch(stockProduct, batch, 50));
        Console.WriteLine($"Після відмови: товар {stockProduct.Quantity}, партія {batch.Quantity} " +
                          "(очікується 100 та 20)");

        Console.WriteLine("\n5. Додаткове №3: дозволені переходи станів");
        Movement movement = Movement.Create("M-001", "P-020", 10);
        Console.WriteLine(movement);
        movement.Confirm();
        Console.WriteLine(movement);
        TryDo("Скасування підтвердженого", () => movement.Cancel());
        Console.WriteLine($"Стан після відмови: {movement.Status}");
        Movement cancelled = Movement.Create("M-002", "P-020", 5);
        cancelled.Cancel();
        Console.WriteLine(cancelled);
    }

    private static void TryDo(string title, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"{title}: ПОМИЛКА — винятку немає");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"{title}: {ex.GetType().Name} — {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"{title}: {ex.GetType().Name} — {ex.Message}");
        }
    }
}
