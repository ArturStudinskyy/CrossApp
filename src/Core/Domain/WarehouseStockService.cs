namespace Core.Domain;

public static class WarehouseStockService
{
    // Навчальна синхронна операція: перевіряємо обидва об'єкти ДО їх зміни.
    public static void IssueFromBatch(Product product, StockBatch batch, int amount)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(batch);
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount,
                "Кількість видачі має бути більшою за нуль");
        if (!string.Equals(product.Id, batch.ProductId, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Партія {batch.Id} не належить товару {product.Id}");
        if (amount > product.Quantity)
            throw new InvalidOperationException(
                $"Недостатній загальний залишок товару {product.Sku}: " +
                $"є {product.Quantity}, запитано {amount}");
        if (amount > batch.Quantity)
            throw new InvalidOperationException(
                $"Недостатній залишок партії {batch.Id}: " +
                $"є {batch.Quantity}, запитано {amount}");

        product.Issue(amount);
        batch.Issue(amount);
    }
}
