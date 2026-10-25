# Запуск із кореня проєкту CrossApp: .\Install-Lab05Extras.ps1
$ErrorActionPreference = 'Stop'
$project = (Get-Location).Path
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$programPath = Join-Path $project 'src\Cli\Program.cs'
$servicePath = Join-Path $project 'src\Core\Services\CatalogService.cs'
$readmePath = Join-Path $project 'README.md'

foreach ($path in @($programPath, $servicePath, $readmePath)) {
    if (-not (Test-Path $path)) { throw "Не знайдено файл: $path. Запусти з кореня CrossApp." }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$program = [System.IO.File]::ReadAllText($programPath)
$service = [System.IO.File]::ReadAllText($servicePath)

# Патч під актуальну структуру CrossApp. Перевіряємо ДО змін.
$oldBlockPattern = '(?s)    ICatalogStore store = useFile\s*\? new FileCatalogStore\(dataPath\)\s*:\s*new InMemoryCatalogStore\(SampleData\.Products\(\)\);\s*var service = new CatalogService\(store\);\s*Lab05Demo\.Run\(service, store\);'
if (-not $program.Contains('Lab05ExtrasDemo.Run(') -and
    -not [regex]::IsMatch($program, $oldBlockPattern)) {
    throw 'Блок Lab05 у Program.cs відрізняється від очікуваного. Нічого не змінено.'
}

$searchAnchor = '    public Product? Find(string id) => _store.GetById(id);'
if (-not $service.Contains('Search(Func<Product, bool>') -and
    -not $service.Contains($searchAnchor)) {
    throw 'Не знайдено точку вставки Search у CatalogService.cs. Нічого не змінено.'
}

# Нові файли не перезаписуємо: залишаємо власні правки, якщо вони вже існують.
$additions = @(
    'src\Core\Storage\CachingCatalogStore.cs',
    'src\Cli\StoreFactory.cs',
    'src\Cli\Lab05ExtrasDemo.cs'
)
foreach ($relative in $additions) {
    $source = Join-Path $root $relative
    $destination = Join-Path $project $relative
    if (Test-Path $destination) {
        Write-Host "Файл уже існує, пропущено: $relative"
        continue
    }
    $parent = Split-Path -Parent $destination
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
    Copy-Item $source $destination
    Write-Host "Додано: $relative"
}

if (-not $service.Contains('Search(Func<Product, bool>')) {
    Copy-Item $servicePath "$servicePath.before-extras.bak" -Force
    $replacement = @'
    public IReadOnlyList<Product> Search(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    public Product? Find(string id) => _store.GetById(id);
'@
    $service = $service.Replace($searchAnchor, $replacement)
    [System.IO.File]::WriteAllText($servicePath, $service, $utf8)
    Write-Host 'Додано Search у CatalogService.cs'
}

if (-not $program.Contains('Lab05ExtrasDemo.Run(')) {
    Copy-Item $programPath "$programPath.before-extras.bak" -Force
    $newBlock = @'
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
'@
    $program = [regex]::Replace($program, $oldBlockPattern, [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $newBlock })
    [System.IO.File]::WriteAllText($programPath, $program, $utf8)
    Write-Host 'Оновлено Program.cs з --extras, --cache, --factory'
}

$readme = [System.IO.File]::ReadAllText($readmePath)
if (-not $readme.Contains('Додаткові завдання лабораторної №5')) {
    $append = @'

### Додаткові завдання лабораторної №5

1. `CachingCatalogStore` — декоратор над `ICatalogStore`, який кешує `List()` та `GetById()` і скидає кеш після `Add`, `Update`, `Remove`.
2. `CatalogService.Search(Func<Product, bool>)` — фільтрація товарів за довільним предикатом.
3. `StoreFactory.Create(args)` — альтернативне створення файлового або оперативного сховища за прапорцем `--file`. Основна точка композиції лишається в `Program.cs`.

Команди перевірки:

```powershell
dotnet run --project src/Cli -- --lab05 --extras
dotnet run --project src/Cli -- --lab05 --extras --cache
dotnet run --project src/Cli -- --lab05 --extras --cache --factory
dotnet run --project src/Cli -- --lab05 --extras --cache --factory --file
```
'@
    [System.IO.File]::AppendAllText($readmePath, $append, $utf8)
    Write-Host 'Оновлено README.md'
}
Write-Host 'Готово. Наступна команда: dotnet build'
