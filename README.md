# CrossApp — Практикум з крос-платформного програмування

**Студент:** Ціздин Роман, група ФЕІ-35  
**Проєкт:** Наскрізна розробка системи обліку (Склад).

## 1. Предметна область
* **Призначення:** Облік продуктів, партій, їх переміщення та контроль термінів придатності.
* **Сутності:** `Product` (товар), `StockBatch` (партія), `Warehouse` (склад), `Movement` (переміщення).

## 2. Структура рішення

Проєкт розділено на ядро бізнес-логіки та інтерфейс користувача:

```text
CrossApp/
├── CrossApp.slnx
├── data/                # Тестові файли (.csv, .json)
└── src/
    ├── Core/            # Бізнес-логіка (Class Library). Не залежить від Cli.
    │   ├── Core.csproj  # Multi-targeting (net8.0; net10.0)
    │   ├── Dto/         # Моделі даних
    │   └── Import/      # Логіка парсингу (CSV, JSON)
    └── Cli/             # Консольний інтерфейс (Console App). Посилається на Core.
        ├── Cli.csproj
        └── Program.cs
```

## 3. Збірка та запуск

**Збірка проєкту:**
```bash
dotnet build
```

**Сценарії запуску:**
```bash
# Імпорт стандартного CSV (data/sample.csv)
dotnet run --project src/Cli

# Імпорт JSON-файлу
dotnet run --project src/Cli -- data/sample.json

# Імпорт змішаного CSV-файлу
dotnet run --project src/Cli -- data/mixed.csv

# Системна інформація
dotnet run --project src/Cli -- --info
```

## 4. Формат даних та обробка помилок

* **CSV:** Кодування UTF-8, роздільник `;`. Заголовок пропускається автоматично.
* **Числа:** Парсяться через `CultureInfo.InvariantCulture`.
* **Відмовостійкість:** Помилкові рядки не "крашать" програму. Результат збирається у `ImportResult<T>`, який містить список успішних записів та масив локалізованих помилок (з номерами рядків).

## 5. Режими публікації (win-x64)

| Режим публікації | Команда (`dotnet publish src/Cli -c Release -r win-x64...`) | Розмір | Потрібен .NET |
| :--- | :--- | :--- | :--- |
| **Framework-dependent** | `--self-contained false` | ~200 КБ | Так |
| **Self-contained** | `--self-contained true` | ~76.6 МБ | Ні |
| **Single-File** | `--self-contained true -p:PublishSingleFile=true` | ~70.1 МБ | Ні |
| **Trimmed** | `... -p:PublishSingleFile=true -p:PublishTrimmed=true` | ~12.5 МБ | Ні |

*Примітка: Trimming суттєво зменшує розмір файлу, але може зламати серіалізацію JSON, оскільки видаляє класи, які викликаються динамічно через рефлексію.*

**Прямий запуск після публікації:**
```cmd
.\publish\self-contained\Cli.exe
.\publish\self-contained\Cli.exe ..\..\data\sample.csv
```

## 6. Multi-targeting
Бібліотека `Core` збирається одночасно під `net8.0` та `net10.0`. Для перевірки використано умовну компіляцію:

```csharp
#if NET10_0_OR_GREATER
    private const string CurrentBuildNote = "Збірка під net10.0";
#else
    private const string CurrentBuildNote = "Збірка під net8.0";
#endif
```