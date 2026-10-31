using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store,
    INotificationSink? notificationSink = null) //dependency injection
{
    private readonly ICatalogStore _store =
        store ?? throw new ArgumentNullException(nameof(store));

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    private void OnChanged(Product product, CatalogChangeKind kind, int delta)
    {
        var change = new CatalogChangedEventArgs(product.Id, kind, delta,
            kind == CatalogChangeKind.Removed ? 0 : product.Quantity);
        Changed?.Invoke(this, change);
        notificationSink?.Notify(change);
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var product = Product.Create(id, sku, name, unit, quantity);
        _store.Add(product);
        OnChanged(product, CatalogChangeKind.Added, quantity);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    public bool Remove(string id)
    {
        var product = _store.GetById(id);
        if (product is null || !_store.Remove(id))
            return false;

        OnChanged(product, CatalogChangeKind.Removed, -product.Quantity);
        return true;
    }

    public IReadOnlyList<Product> All() => _store.List();

    public IReadOnlyList<Product> Search(Func<Product, bool> predicate) // предикат
        => FindAll(predicate);

    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    public Product? Find(string id) => _store.GetById(id);
}
