using Core.Abstractions;
using Core.Services;
using Core.Storage;

internal static class Lab05ExtrasDemo
{
    public static void Run(
        CatalogService service,
        ICatalogStore store,
        ICatalogStore baseStore,
        bool useFactory)
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

        service.Find(id); // перше або повторне отримання
        service.Find(id); // гарантовано повторне читання з кешу

        Console.WriteLine($"Влучань кешу після двох пошуків: {cache.CacheHits - hitsBefore}");
        Console.WriteLine($"Усього CacheHits = {cache.CacheHits}; CacheMisses = {cache.CacheMisses}");

        int before = service.Find(id)!.Quantity;
        service.Receive(id, 2); // Update скидає кеш
        int after = service.Find(id)!.Quantity;

        Console.WriteLine($"Перевірка скидання кешу після Receive: {before} -> {after}");
        Console.WriteLine($"Оновлення коректне: {after == before + 2}");
    }
}
