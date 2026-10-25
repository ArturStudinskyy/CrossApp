using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Декоратор: кешує читання та передає запис вкладеному сховищу.
public sealed class CachingCatalogStore(ICatalogStore inner) : ICatalogStore
{
    private readonly ICatalogStore _inner =
        inner ?? throw new ArgumentNullException(nameof(inner));

    private readonly Dictionary<string, Product?> _byId =
        new(StringComparer.OrdinalIgnoreCase);

    private IReadOnlyList<Product>? _all;

    public int CacheHits { get; private set; }
    public int CacheMisses { get; private set; }

    public IReadOnlyList<Product> List()
    {
        if (_all is not null)
        {
            CacheHits++;
            return _all;
        }

        CacheMisses++;
        _all = _inner.List();
        return _all;
    }

    public Product? GetById(string id)
    {
        if (_byId.TryGetValue(id, out Product? cached))
        {
            CacheHits++;
            return cached;
        }

        CacheMisses++;
        Product? product = _inner.GetById(id);
        _byId[id] = product;
        return product;
    }

    public void Add(Product item)
    {
        _inner.Add(item);
        Invalidate();
    }

    public void Update(Product item)
    {
        _inner.Update(item);
        Invalidate();
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        if (removed)
            Invalidate();
        return removed;
    }

    private void Invalidate()
    {
        _byId.Clear();
        _all = null;
    }
}
