using Core;
using Core.Abstractions;
using Core.Storage;

// Фабрика — альтернативний спосіб створювати реалізації ICatalogStore.
internal static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        bool useFile = args.Any(a => a.Equals(
            "--file", StringComparison.OrdinalIgnoreCase));

        string dataPath = Path.Combine(
            AppContext.BaseDirectory, "data", "catalog.json");

        return useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());
    }
}
