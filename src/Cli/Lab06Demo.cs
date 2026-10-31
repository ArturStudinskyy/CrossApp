using Cli.Notifications;
using Core.Abstractions;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;

internal static class Lab06Demo
{
    public static void Run(CatalogService service, ICatalogStore store, bool extras)
    {
        Console.WriteLine("=== Лабораторна №6: події, делегати та лямбди ===");
        Console.WriteLine($"Сховище: {store.GetType().Name}");
        string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
        var console = new ConsoleSink();
        var file = new FileSink(logPath);
        int changes = 0;

        void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
        {
            console.Notify(e);
            file.Notify(e);
            Console.WriteLine($"  Підписник 1: лічильник ще {changes}");
        }
        EventHandler<CatalogChangedEventArgs> counter = (_, _) =>
        {
            changes++;
            Console.WriteLine($"  Підписник 2: лічильник уже {changes}");
        };

        service.Changed += OnCatalogChanged;
        service.Changed += counter;
        try
        {
            var product = service.Add("ARO-MILK-L06", "Молоко ARO", "шт", 50);
            service.Receive(product.Id, 25);
            service.Issue(product.Id, 10);
            service.Receive(product.Id, 5);
            Console.WriteLine($"Подій отримано: {changes} (очікується 4)");

            Console.WriteLine("\nВідмови: жодна не повинна породити подію");
            TryRejected("Від'ємний прихід", () => service.Receive(product.Id, -5));
            TryRejected("Невідомий id", () => service.Receive("missing-lab06", 5));
            TryRejected("Перевитрата", () => service.Issue(product.Id, 1000));
            Console.WriteLine($"Після відмов: подій {changes}, залишок {service.Find(product.Id)!.Quantity}");

            Console.WriteLine("\nВідписка консолі та файла: лишається лічильник");
            service.Changed -= OnCatalogChanged;
            service.Receive(product.Id, 5);
            Console.WriteLine($"Подій отримано: {changes} (очікується 5), залишок 75");

            Console.WriteLine("\nВідписка лічильника: операція без підписників");
            service.Changed -= counter;
            service.Issue(product.Id, 70);
            Console.WriteLine($"Лічильник: {changes}; залишок: {service.Find(product.Id)!.Quantity} (очікується 5)");
            Console.WriteLine("FindAll(p => p.Quantity < 10):");
            foreach (var item in service.FindAll(p => p.Quantity < 10))
                Console.WriteLine($"  {item}");

            Console.WriteLine("\nВидалення: повторна підписка та одна подія Removed");
            service.Changed += OnCatalogChanged;
            service.Changed += counter;
            Console.WriteLine($"Видалено: {service.Remove(product.Id)}");
            Console.WriteLine($"Повторне видалення: {service.Remove(product.Id)} (події немає)");
            Console.WriteLine($"Усього подій лічильника: {changes} (очікується 6)");
        }
        finally
        {
            service.Changed -= OnCatalogChanged;
            service.Changed -= counter;
        }
        Console.WriteLine($"Журнал: {logPath}");
        if (extras)
            RunExtras();
    }

    private static void RunExtras()
    {
        Console.WriteLine("\n=== Додаткове №1: INotificationSink через конструктор ===");
        string path = Path.Combine(AppContext.BaseDirectory, "logs", "sink.log");
        INotificationSink[] sinks = [new ConsoleSink(), new FileSink(path)];
        foreach (var sink in sinks)
        {
            Console.WriteLine($"Реалізація: {sink.GetType().Name}; підписок на event немає");
            var service = new CatalogService(new InMemoryCatalogStore(), sink);
            var item = service.Add("ARO-TEA", "Чай ARO", "уп", 10);
            service.Receive(item.Id, 2);
        }
        Console.WriteLine($"Журнал альтернативи DI: {path}");

        Console.WriteLine("\n=== Додаткове №2: Action<string> у методі імпорту ===");
        int skipped = 0;
        string csvPath = Path.Combine(AppContext.BaseDirectory, "data", "sample.csv");
        var result = ProductCsvImporter.Load(csvPath, message =>
        {
            skipped++;
            Console.WriteLine($"  Action: {message}");
        });
        Console.WriteLine($"Прийнято {result.Items.Count}; пропущено {skipped}; помилок {result.Errors.Count}");
    }

    private static void TryRejected(string title, Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"{title}: {ex.Message}");
            return;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"{title}: {ex.Message}");
            return;
        }
        throw new InvalidOperationException($"Перевірка '{title}' не спричинила відмови.");
    }
}
