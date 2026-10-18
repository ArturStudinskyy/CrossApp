# CrossApp — Практикум з крос-платформного програмування

**Студент:** Студинський Артур, група ФЕІ-35  
**Семестровий проєкт:** Наскрізна розробка модульної системи обліку (тижні 1–16).

---

## 1. Предметна область (Domain)

* **Назва:** Склад (Warehouse Management System — продовольчі товари METRO).
* **Призначення:** Облік товарів на складі, контроль залишків, надходжень та переміщень між складськими локаціями.
* **Ключові сутності:**
  * `Product` — товар або номенклатурна позиція складу.
  * `StockBatch` — партія товару з інформацією про кількість та дату надходження.
  * `Warehouse` — склад або складська локація.
  * `Movement` — операція надходження, переміщення або списання товарів.

---

## 2. Архітектура та структура рішення (Solution)

Проєкт організовано за принципом відокремлення консольного інтерфейсу від основної логіки:

```text id="1dvvan"
CrossApp/
├── CrossApp.slnx
├── README.md
├── .gitignore
├── data/
│   ├── sample.csv
│   ├── correct.csv
│   ├── sample.json
│   └── mixed.csv
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   ├── Dto/
    │   │   ├── ImportEntityDto.cs
    │   │   ├── ProductDto.cs
    │   │   ├── WarehouseDto.cs
    │   │   └── ImportResult.cs
    │   └── Import/
    │       ├── ProductCsvImporter.cs
    │       ├── ProductJsonImporter.cs
    │       └── MixedCsvImporter.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```

### Призначення компонентів

* **`src/Core` (Class Library):**
  * містить DTO-моделі та логіку імпорту;
  * містить інформацію про середовище виконання;
  * компілюється у `.dll`;
  * не залежить від `Cli`.

* **`src/Cli` (Console Application):**
  * обробляє аргументи командного рядка;
  * перевіряє існування файлу;
  * вибирає потрібний імпортер;
  * виводить записи, помилки та статистику.

* **Правило залежностей:** `Cli → Core`.

---

## 3. Інструкція зі збірки та запуску

### Збірка всієї системи

```bash id="wyu45b"
dotnet build
```

### Збірка бібліотеки Core

```bash id="b9i1ow"
dotnet build src/Core/Core.csproj
```

### Основні сценарії запуску

```bash id="xm9x73"
# Імпорт стандартного CSV-файлу
dotnet run --project src/Cli

# Імпорт коректного CSV-файлу
dotnet run --project src/Cli -- data/correct.csv

# Імпорт JSON-файлу
dotnet run --project src/Cli -- data/sample.json

# Імпорт змішаного CSV
dotnet run --project src/Cli -- data/mixed.csv

# Перегляд інформації про середовище
dotnet run --project src/Cli -- --info
```

---

## 4. Формати даних та імпорт

* **Кодування:** UTF-8.
* **Роздільник CSV:** `;`.
* **Основний формат:**

```text id="9dj7vn"
id;sku;name;unit;quantity
```

Приклад:

```text id="ktsb37"
P-001;ARO-MILK-900;Молоко ARO 2.5% 900мл;шт;48
```

Для представлення даних використовуються `record`-типи:

* `ProductDto` — інформація про товар;
* `WarehouseDto` — інформація про склад;
* `ImportResult<T>` — успішно імпортовані записи та список помилок.

Розбір CSV реалізовано через `switch expression` і pattern matching. Використовуються list patterns, property patterns, `or`, `when` та `TryParse`.

Пошкоджений рядок не припиняє весь імпорт. Помилка додається до списку `Errors` разом із номером рядка.

Приклад статистики:

```text id="6tptbd"
Статистика: усього 13 / прийнято 10 / пропущено 3 / помилок 23,08%
```

### Додаткові завдання

Реалізовано:

* імпорт JSON через `System.Text.Json`;
* змішаний CSV:
  * `P` — товар;
  * `W` — склад;
* статистику імпорту: усього / прийнято / пропущено / % помилок.

---

## 5. Дослідження режимів публікації (.NET Publish)

Під час попередніх лабораторних робіт досліджено способи публікації застосунку для `win-x64`.

| Режим публікації | Розмір каталогу | Потрібен .NET Runtime |
| :--- | ---: | :---: |
| **Framework-dependent** | ≈ 199 КБ | Так |
| **Self-contained** | ≈ 76.6 МБ | Ні |
| **Single-File** | ≈ 70.1 МБ | Ні |
| **Trimmed Single-File** | ≈ 12.5 МБ | Ні |

### Framework-dependent

```bash id="ooei7j"
dotnet publish src/Cli -c Release -r win-x64 --self-contained false
```

### Self-contained

```bash id="zkjx8z"
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
```

### Single-File

```bash id="u6clpb"
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Trimmed Single-File

```bash id="w1yg9n"
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

---

## 6. Multi-targeting та умовна компіляція

Бібліотеку `src/Core` налаштовано на збірку під дві версії платформи:

```xml id="8tayzv"
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```

Після виконання:

```bash id="pd4aq2"
dotnet build
```

формуються окремі збірки:

```text id="894ygf"
src/Core/bin/Debug/net8.0/Core.dll
src/Core/bin/Debug/net10.0/Core.dll
```

У коді використовується умовна компіляція:

```csharp id="fknku3"
#if NET10_0_OR_GREATER
    private const string CurrentBuildNote = "Збірка під net10.0";
#else
    private const string CurrentBuildNote = "Збірка під net8.0";
#endif
```

Це дозволяє компілювати бібліотеку `Core` під декілька версій .NET без дублювання проєкту.

## Лабораторна №4 — Доменна модель складу

У `Core.Domain` створено доменні сутності `Product`, `StockBatch`, `Movement` із
приватними конструкторами, фабриками та захищеним станом. DTO з лабораторної №3
збережено: `Product.ToDto()` і `Product.FromDto()` підтримують також поле `Note`.

### Інваріанти

- Id, SKU, назва товару й одиниця вимірювання не можуть бути порожніми (`ArgumentException`).
- Початкова кількість не може бути від'ємною (`ArgumentOutOfRangeException`).
- Прихід і видача мають бути додатними (`ArgumentOutOfRangeException`).
- Не можна видати більше, ніж є на складі або в партії (`InvalidOperationException`).
- Партія повинна належати обраному товару (`InvalidOperationException`).
- `Movement` дозволяє лише переходи Draft → Confirmed або Draft → Cancelled (`InvalidOperationException`).

### Додаткові завдання

1. `ProductDomainImporter.Convert` обробляє `ImportResult<ProductDto>` і повертає
   `ImportResult<Product>` з успішними сутностями та помилками.
2. `WarehouseStockService.IssueFromBatch` перевіряє зв'язок і залишки двох сутностей.
3. `MovementStatus` та `Movement.ChangeStatus` реалізують переходи через `switch expression`.

### Запуск

```powershell
dotnet build
dotnet run --project src/Cli -- --lab04
```

Демонстрація містить успішний сценарій, порушення інваріантів і всі три додаткові завдання.
