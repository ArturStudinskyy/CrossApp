using Core.Abstractions;
using Core.Services;
using Core.Storage;

internal static class Lab05Demo
{
    public static void Run(CatalogService service, ICatalogStore store,
        ICatalogStore baseStore, bool useFactory, bool extras)
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

        if (extras)
            RunExtras(service, store, baseStore, useFactory);
    }

    private static void RunExtras(CatalogService service, ICatalogStore store,
        ICatalogStore baseStore, bool useFactory)
    {
        Console.WriteLine("\n=== ДОДАТКОВІ ЗАВДАННЯ №5 ===");

        Console.WriteLine("\n1. Пошук через Func<Product, bool>");
        var many = service.Search(p => p.Quantity >= 40);
        Console.WriteLine($"Товарів із залишком >= 40: {many.Count}");
        foreach (var product in many.Take(5))
            Console.WriteLine($"  {product.Sku}: {product.Quantity} {product.Unit}");

        var milk = service.Search(p => p.Name.Contains(
            "Молоко", StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"Збігів за назвою 'Молоко': {milk.Count}");

        Console.WriteLine("\n2. Фабрика StoreFactory.Create(args)");
        Console.WriteLine($"Режим поточного запуску: {(useFactory ? "фабрика" : "вибір у Program.cs")}");
        Console.WriteLine($"Базове сховище: {baseStore.GetType().Name}");
        Console.WriteLine($"Фабрика без --file: {StoreFactory.Create(["--lab05"]).GetType().Name}");
        Console.WriteLine($"Фабрика з --file: {StoreFactory.Create(["--lab05", "--file"]).GetType().Name}");

        Console.WriteLine("\n3. Кешувальний декоратор");
        if (store is not CachingCatalogStore cache)
        {
            Console.WriteLine("Декоратор не ввімкнено. Додай параметр --cache.");
            return;
        }

        string id = service.All().First().Id;
        int hitsBefore = cache.CacheHits;

        service.Find(id);
        service.Find(id);
        Console.WriteLine($"Влучань кешу після двох пошуків: {cache.CacheHits - hitsBefore}");
        Console.WriteLine($"Усього CacheHits = {cache.CacheHits}; CacheMisses = {cache.CacheMisses}");

        int before = service.Find(id)!.Quantity;
        service.Receive(id, 2);
        int after = service.Find(id)!.Quantity;

        Console.WriteLine($"Перевірка скидання кешу після Receive: {before} -> {after}");
        Console.WriteLine($"Оновлення коректне: {after == before + 2}");
    }
}
