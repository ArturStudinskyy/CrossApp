namespace Core.Events;

public enum CatalogChangeKind { Added, Received, Issued, Removed }

public sealed class CatalogChangedEventArgs(
    string id, CatalogChangeKind kind, int delta, int quantityAfter) : EventArgs
{
    public string Id { get; } = id;
    public CatalogChangeKind Kind { get; } = kind;
    public int Delta { get; } = delta;
    public int QuantityAfter { get; } = quantityAfter;
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}
