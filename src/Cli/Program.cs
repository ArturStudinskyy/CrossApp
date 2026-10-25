using System.Text;
using System.Text.Json;
using Core;
using Core.Dto;
using Core.Import;
using Core.Abstractions;
using Core.Storage;
using Core.Services;

Console.OutputEncoding = Encoding.UTF8;

if (args.Any(a => a.Equals("--lab04",
    StringComparison.OrdinalIgnoreCase)))
{
    Lab04Demo.Run();
    return 0;
}

if (args.Any(a => a.Equals("--lab05",
    StringComparison.OrdinalIgnoreCase)))
{
    bool useFile = args.Any(a => a.Equals(
        "--file", StringComparison.OrdinalIgnoreCase));

    string dataPath = Path.Combine(
        AppContext.BaseDirectory,
        "data",
        "catalog.json"
    );

    bool useFactory = args.Any(a => a.Equals(
        "--factory", StringComparison.OrdinalIgnoreCase));
    bool useCache = args.Any(a => a.Equals(
        "--cache", StringComparison.OrdinalIgnoreCase));

    ICatalogStore baseStore = useFactory
        ? StoreFactory.Create(args)
        : (useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products()));

    ICatalogStore store = useCache
        ? new CachingCatalogStore(baseStore)
        : baseStore;

    var service = new CatalogService(store);
    Lab05Demo.Run(service, store);

    if (args.Any(a => a.Equals("--extras",
        StringComparison.OrdinalIgnoreCase)))
    {
        Lab05ExtrasDemo.Run(service, store, baseStore, useFactory);
    }
    return 0;
}

bool infoMode = args.Any(
    a => a.Equals(
        "--info",
        StringComparison.OrdinalIgnoreCase
    )
);

if (infoMode)
{
    bool jsonMode = args.Any(
        a => a.Equals(
            "--json",
            StringComparison.OrdinalIgnoreCase
        )
    );

    return PrintEnvironmentInfo(jsonMode);
}

string? pathArgument = args.FirstOrDefault(
    a => !a.StartsWith("--", StringComparison.Ordinal)
);

string path = pathArgument
    ?? Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine(
        $"Файл не знайдено: {Path.GetFullPath(path)}"
    );

    return 1;
}

string extension = Path
    .GetExtension(path)
    .ToLowerInvariant();

bool isMixedFile = Path
    .GetFileName(path)
    .Equals(
        "mixed.csv",
        StringComparison.OrdinalIgnoreCase
    );

return (extension, isMixedFile) switch
{
    (".json", _) => RunJson(path),

    (".csv", true) => RunMixedCsv(path),

    (".csv", false) => RunCsv(path),

    _ => UnsupportedFormat(extension)
};


static int RunCsv(string path)
{
    ImportResult<ProductDto> result =
        ProductCsvImporter.Load(path);

    return PrintProducts(result);
}


static int RunJson(string path)
{
    ImportResult<ProductDto> result =
        ProductJsonImporter.Load(path);

    return PrintProducts(result);
}


static int RunMixedCsv(string path)
{
    ImportResult<ImportEntityDto> result =
        MixedCsvImporter.Load(path);

    Console.WriteLine(
        $"Завантажено записів: {result.Items.Count}"
    );

    foreach (ImportEntityDto item in result.Items.Take(5))
    {
        string text = item switch
        {
            ProductDto p =>
                $"Товар: {p.Id,-6} {p.Sku,-18} " +
                $"{p.Name,-40} {p.Quantity,5} {p.Unit}",

            WarehouseDto w =>
                $"Склад: {w.Id,-6} {w.Name,-25} {w.City}",

            _ => "Невідомий тип"
        };

        Console.WriteLine(text);
    }

    PrintErrors(result.Errors);

    PrintStatistics(
        result.Items.Count,
        result.Errors.Count
    );

    return 0;
}


static int PrintProducts(
    ImportResult<ProductDto> result
)
{
    Console.WriteLine(
        $"Завантажено записів: {result.Items.Count}"
    );

    foreach (ProductDto product in result.Items.Take(5))
    {
        Console.WriteLine(
            $" {product.Id,-6} " +
            $"{product.Sku,-18} " +
            $"{product.Name,-40} " +
            $"{product.Quantity,5} " +
            $"{product.Unit}"
        );
    }

    PrintErrors(result.Errors);

    PrintStatistics(
        result.Items.Count,
        result.Errors.Count
    );

    return 0;
}


static void PrintErrors(
    IReadOnlyList<string> errors
)
{
    if (errors.Count == 0)
    {
        return;
    }

    Console.WriteLine(
        $"Пропущено рядків: {errors.Count}"
    );

    foreach (string error in errors)
    {
        Console.WriteLine($" ! {error}");
    }
}


static void PrintStatistics(
    int accepted,
    int skipped
)
{
    int total = accepted + skipped;

    double errorPercent =
        total == 0
            ? 0
            : (double)skipped / total * 100;

    Console.WriteLine(
        $"Статистика: усього {total} / " +
        $"прийнято {accepted} / " +
        $"пропущено {skipped} / " +
        $"помилок {errorPercent:F2}%"
    );
}


static int UnsupportedFormat(string extension)
{
    Console.WriteLine(
        $"Непідтримуваний формат файлу: {extension}"
    );

    return 1;
}


static int PrintEnvironmentInfo(bool jsonMode)
{
    EnvironmentReport report =
        EnvironmentInfo.Collect();

    if (jsonMode)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder =
                System.Text.Encodings.Web
                    .JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };

        Console.WriteLine(
            JsonSerializer.Serialize(
                report,
                options
            )
        );

        return 0;
    }

    Console.WriteLine(report.Title);
    Console.WriteLine(
        $"Студент: {report.Student}"
    );

    Console.WriteLine(
        new string('.', 67)
    );

    Console.WriteLine(
        $"ОС                 : {report.OsDescription}"
    );

    Console.WriteLine(
        $"Архітектура        : {report.ProcessArchitecture}"
    );

    Console.WriteLine(
        $"RID                : {report.DetectedRid}"
    );

    Console.WriteLine(
        $".NET               : {report.FrameworkDescription}"
    );

    Console.WriteLine(
        $"Предметна область  : {report.Domain}"
    );

    return 0;
}