using Core.Domain;

namespace Core.Reports;

public sealed record StockLine(string Sku, string Name, int Quantity, string Unit);
public sealed record GroupSummary(string Key, int Count, int TotalQuantity);
public sealed record MovementLine(string Sku, string MovementId, int Quantity);

public static class ReportExtensions
{
    // Сортування за показником і стабільний другий ключ для різних джерел.
    public static IEnumerable<T> TopBy<T>(this IEnumerable<T> source,
        Func<T, int> score, Func<T, string> tieBreaker, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(score);
        ArgumentNullException.ThrowIfNull(tieBreaker);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return source.OrderByDescending(score)
            .ThenBy(tieBreaker, StringComparer.Ordinal).Take(count);
    }
}

public static class ReportService
{
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Where(p => p.Quantity < threshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .ThenBy(p => p.Id, StringComparer.Ordinal)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();
    }

    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.TopBy(p => p.Quantity, p => p.Id, count)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();
    }

    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
            .OrderByDescending(g => g.TotalQuantity)
            .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static (int Items, int Total, double Average, int Max) Totals(
        IEnumerable<Product> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity),
                list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }

    // Додаткове 1: Join двох послідовностей із проєкцією у DTO.
    public static IReadOnlyList<MovementLine> TopMovements(
        IEnumerable<Product> products, IEnumerable<Movement> movements, int count)
    {
        ArgumentNullException.ThrowIfNull(products);
        ArgumentNullException.ThrowIfNull(movements);
        return products.Join(movements, p => p.Id, m => m.ProductId,
                (p, m) => new MovementLine(p.Sku, m.Id, m.Quantity))
            .TopBy(m => m.Quantity, m => m.MovementId, count)
            .ToList();
    }

    // Додаткове 3: еквівалентний запит у синтаксисі from/group/orderby/select.
    public static IReadOnlyList<GroupSummary> ByUnitQuery(IEnumerable<Product> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var query = from product in source
                    group product by product.Unit.ToLowerInvariant() into g
                    let total = g.Sum(p => p.Quantity)
                    orderby total descending, g.Key
                    select new GroupSummary(g.Key, g.Count(), total);
        return query.ToList();
    }
}
