using System.Diagnostics;
using Core.Domain;
using Core.Reports;
using Core.Services;

internal static class Lab07Demo
{
    public static int Run(CatalogService service, string[] args)
    {
        string command = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal))
            ?? "report-top";
        var items = service.All();
        Console.WriteLine($"=== Лабораторна №7: {command} ===");
        switch (command)
        {
            case "report-top":
                Print(ReportService.TopByQuantity(items, 5));
                break;
            case "report-low":
                Print(ReportService.LowStock(items, 10));
                break;
            case "report-groups":
                PrintGroups(ReportService.ByUnit(items));
                break;
            case "report-sum":
                var totals = ReportService.Totals(items);
                Console.WriteLine($"  позицій {totals.Items}, всього {totals.Total}, " +
                    $"середнє {totals.Average:F2}, макс {totals.Max}");
                break;
            default:
                Console.Error.WriteLine("Команди: report-top | report-low | report-groups | report-sum");
                return 1;
        }

        if (args.Any(a => a.Equals("--demo", StringComparison.OrdinalIgnoreCase)) ||
            args.Any(a => a.Equals("--extras", StringComparison.OrdinalIgnoreCase)))
            ShowDeferred(service);

        if (args.Any(a => a.Equals("--extras", StringComparison.OrdinalIgnoreCase)))
            ShowExtras(items);
        return 0;
    }

    private static void Print(IReadOnlyList<StockLine> lines)
    {
        if (lines.Count == 0) Console.WriteLine("  Немає записів.");
        foreach (var line in lines)
            Console.WriteLine($"  {line.Sku,-20} {line.Name,-30} {line.Quantity,6} {line.Unit}");
    }

    private static void PrintGroups(IReadOnlyList<GroupSummary> groups)
    {
        if (groups.Count == 0) Console.WriteLine("  Немає записів.");
        foreach (var group in groups)
            Console.WriteLine($"  {group.Key,-6} позицій: {group.Count,3}  сумарно: {group.TotalQuantity,6}");
    }

    private static void ShowDeferred(CatalogService service)
    {
        Console.WriteLine("\nДемонстрація відкладеного виконання:");
        var source = service.All().ToList(); // All повертає знімок; змінюємо локальний список.
        var query = source.Where(p => p.Quantity < 10);
        Console.WriteLine($"Дефіцитних до змін: {query.Count()}");
        source.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3));
        Console.WriteLine($"Дефіцитних після змін: {query.Count()}");
        var snapshot = query.ToList();
        source.Clear();
        Console.WriteLine($"У знімку лишилось: {snapshot.Count}");
    }

    private static void ShowExtras(IReadOnlyList<Product> items)
    {
        Console.WriteLine("\nДодаткове 1: Join товарів і переміщень, TopBy для другого звіту:");
        var movements = items.Take(3).Select((p, i) =>
            Movement.Create($"M-{i + 1:000}", p.Id, (i + 1) * 5)).ToList();
        foreach (var line in ReportService.TopMovements(items, movements, 5))
            Console.WriteLine($"  {line.Sku,-20} {line.MovementId,-8} {line.Quantity,5}");

        Console.WriteLine("\nДодаткове 3: групування синтаксисом запиту:");
        var groups = ReportService.ByUnitQuery(items);
        PrintGroups(groups);
        Console.WriteLine($"Підсумки двох синтаксисів однакові: " +
            $"{groups.Select(g => (g.Key.ToLowerInvariant(), g.Count, g.TotalQuantity))
                .SequenceEqual(ReportService.ByUnit(items).Select(g =>
                    (g.Key.ToLowerInvariant(), g.Count, g.TotalQuantity)))}");

        Console.WriteLine("\nДодаткове 4: Stopwatch для 100 000 товарів:");
        var source = Enumerable.Range(1, 100_000)
            .Select(i => Product.Create($"B-{i:000000}", $"SKU-{i:000000}",
                "Тестовий товар", "шт", i % 120)).ToList();
        if (!WithoutIntermediateList(source).SequenceEqual(WithIntermediateList(source)))
            throw new InvalidOperationException("Варіанти запиту повернули різні результати.");
        // Прогрів JIT; обидва варіанти повертають той самий топ-5.
        _ = WithoutIntermediateList(source);
        _ = WithIntermediateList(source);
        long without = MedianMilliseconds(() => WithoutIntermediateList(source));
        long with = MedianMilliseconds(() => WithIntermediateList(source));
        Console.WriteLine($"  Без проміжного ToList: {without} мс");
        Console.WriteLine($"  З проміжним ToList: {with} мс");
        Console.WriteLine("  Тривалість залежить від комп'ютера та стану пам'яті.");
    }

    private static IReadOnlyList<StockLine> WithoutIntermediateList(IEnumerable<Product> source) =>
        source.Where(p => p.Quantity >= 10)
            .TopBy(p => p.Quantity, p => p.Sku, 5)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit)).ToList();

    private static IReadOnlyList<StockLine> WithIntermediateList(IEnumerable<Product> source) =>
        source.Where(p => p.Quantity >= 10).ToList()
            .TopBy(p => p.Quantity, p => p.Sku, 5)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit)).ToList();

    private static long MedianMilliseconds(Func<IReadOnlyList<StockLine>> action)
    {
        var samples = new long[5];
        for (int i = 0; i < samples.Length; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = action();
            stopwatch.Stop();
            if (result.Count != 5) throw new InvalidOperationException("Некоректний результат вимірювання.");
            samples[i] = stopwatch.ElapsedMilliseconds;
        }
        Array.Sort(samples);
        return samples[samples.Length / 2];
    }
}
