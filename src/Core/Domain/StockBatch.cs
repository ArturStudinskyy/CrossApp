namespace Core.Domain;

public sealed class StockBatch
{
    private int _quantity;

    public string Id { get; }
    public string ProductId { get; }
    public DateTime ReceivedOn { get; }
    public int Quantity => _quantity;

    private StockBatch(string id, string productId, DateTime receivedOn, int quantity)
    {
        Id = id;
        ProductId = productId;
        ReceivedOn = receivedOn;
        _quantity = quantity;
    }

    public static StockBatch Create(string id, string productId,
                                    DateTime receivedOn, int quantity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Номер партії обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Ідентифікатор товару обов'язковий", nameof(productId));
        if (receivedOn == default)
            throw new ArgumentException("Дата надходження обов'язкова", nameof(receivedOn));
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Залишок партії не може бути від'ємним");

        return new StockBatch(id.Trim(), productId.Trim(), receivedOn, quantity);
    }

    public void Issue(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount,
                "Кількість видачі має бути більшою за нуль");
        if (amount > _quantity)
            throw new InvalidOperationException(
                $"Недостатній залишок партії {Id}: є {_quantity}, запитано {amount}");
        _quantity -= amount;
    }
}
