using Core.Abstractions;
using Core.Services;

internal static class Lab05Demo
{
    public static void Run(CatalogService service, ICatalogStore store)
    {
        Console.WriteLine("=== Лабораторна №5: сервісний шар та ручний DI ===");
        Console.WriteLine($"Сховище: {store.GetType().Name}");

        Console.WriteLine("\n1. Додавання, прихід і видача товару");
        var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
        service.Receive(created.Id, 25);
        service.Issue(created.Id, 10);
        Console.WriteLine($"Створено: {created} (очікується 65 м)");

        Console.WriteLine("\n2. Пошук за id");
        Console.WriteLine($"Знайдено: {service.Find(created.Id)}");

        Console.WriteLine("\n3. Список товарів");
        foreach (var product in service.All())
            Console.WriteLine($"  {product}");
        Console.WriteLine($"Кількість товарів: {service.All().Count}");

        Console.WriteLine("\n4. Сценарії відмови");
        try
        {
            service.Receive("missing-id", 5);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Невідомий id: {ex.Message}");
        }

        try
        {
            store.Add(created);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Дубль id: {ex.Message}");
        }
    }
}
