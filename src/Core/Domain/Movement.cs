namespace Core.Domain;

public sealed class Movement
{
    public string Id { get; }
    public string ProductId { get; }
    public int Quantity { get; }
    public MovementStatus Status { get; private set; }

    private Movement(string id, string productId, int quantity)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        Status = MovementStatus.Draft;
    }

    public static Movement Create(string id, string productId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Номер переміщення обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Ідентифікатор товару обов'язковий", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Кількість переміщення має бути більшою за нуль");
        return new Movement(id.Trim(), productId.Trim(), quantity);
    }

    public void ChangeStatus(MovementStatus newStatus)
    {
        bool allowed = (Status, newStatus) switch
        {
            (MovementStatus.Draft, MovementStatus.Confirmed) => true,
            (MovementStatus.Draft, MovementStatus.Cancelled) => true,
            _ => false
        };
        if (!allowed)
            throw new InvalidOperationException(
                $"Недопустимий перехід стану: {Status} -> {newStatus}");
        Status = newStatus;
    }

    public void Confirm() => ChangeStatus(MovementStatus.Confirmed);
    public void Cancel() => ChangeStatus(MovementStatus.Cancelled);

    public override string ToString() =>
        $"Переміщення {Id}: {Quantity} од., стан {Status}";
}
