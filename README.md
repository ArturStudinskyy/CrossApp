# CrossApp — Практикум з крос-платформного програмування

**Поточний етап: лабораторна №6 (основні та обидва додаткові завдання).**

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

## Лабораторна №5 — Сервісний шар та ручний DI

Реалізовано інтерфейс ICatalogStore та дві реалізації сховища:

- InMemoryCatalogStore — зберігання в оперативній пам'яті.
- FileCatalogStore — зберігання в JSON-файлі.

CatalogService реалізує додавання товару, надходження, видачу, пошук і перегляд каталогу.

Залежності передаються через конструктор. Вибір сховища здійснюється в Program.cs без DI-контейнера.

### Команди запуску

dotnet build

dotnet run --project src/Cli -- --lab05

dotnet run --project src/Cli -- --lab05 --file

Перевірено роботу двох сховищ, збереження даних у JSON і обробку помилок.

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

## Лабораторна №6 — Події, Action/Func та лямбди

`CatalogService.Changed` має тип `EventHandler<CatalogChangedEventArgs>`.
Дані події: `Id`, `Kind`, `Delta`, `QuantityAfter`, `At`.
`Added`, `Received`, `Issued`, `Removed` виникають лише після успішного запису
у сховище. Для `Removed` дельта дорівнює мінус останній залишок, а залишок після
видалення — 0. Повторне видалення повертає `false` і не породжує події.
`FindAll(Func<Product, bool>)` повертає матеріалізований результат пошуку;
`Search` із лабораторної №5 збережено як сумісний метод.

Точка композиції залишається у `Program.cs`, демонстрація винесена у `Lab06Demo.cs`.
Перший підписник — локальний метод, який пише в консоль та файл; другий —
збережена лямбда із замиканням над лічильником. Демо показує порядок викликів,
відписку одного, потім обох підписників, відмови інваріантів і видалення.
Core не виводить повідомлень у консоль і не пише журналів.

```powershell
dotnet build
dotnet run --project src/Cli -- --lab06
dotnet run --project src/Cli -- --lab06 --extras
dotnet run --project src/Cli -- --lab06 --extras --file --cache --factory
```

Додаткові завдання:

1. `INotificationSink` у Core та `ConsoleSink` / `FileSink` у Cli.
   У додатковому демо Cli передає кожну реалізацію в конструктор сервісу;
   підписки на `Changed` у цьому окремому сценарії відсутні.
2. `ProductCsvImporter.Load(path, Action<string>? onSkipped = null)` викликає
   делегат один раз для кожного пропущеного некоректного рядка. Збережено
   колекцію `Errors` та можливість виклику без делегата.

Журнали створюються відносно `AppContext.BaseDirectory`:
`src/Cli/bin/Debug/net10.0/logs/app.log` і `logs/sink.log` при Debug-запуску.
Кожен запуск дописує 5 рядків основного демо, а `--extras` — ще 2 рядки
в окремий `sink.log`. Каталоги створює код; файли журналів і результати
збірки виключені з Git. Приклад журналу можна отримати після запуску демо.
Помилки доступу до файла обробляє `FileSink`, щоб вони не перервали
наступного підписника. Довільний сторонній обробник, який кидає виняток,
може перервати синхронний виклик події; виконана зміна даних не скасовується.

### Перевірка

Збірка проходить без помилок і попереджень. Перевірено основний сценарій,
обидва додаткові завдання, файлове сховище, кеш і фабрику, а також запуск
попередніх лабораторних. Команди для повторення наведені вище.

Логіка сповіщень Cli зібрана у NotificationSinks.cs.

## Лабораторна №7 — звіти через LINQ to Objects

`ReportService` у Core приймає `IEnumerable<Product>` і повертає DTO, тому
логіка звітів не залежить від сховища та способу виведення. У Cli доступні
чотири підкоманди:

```powershell
dotnet run --project src/Cli -- report-top
dotnet run --project src/Cli -- report-low
dotnet run --project src/Cli -- report-groups
dotnet run --project src/Cli -- report-sum
dotnet run --project src/Cli -- --lab07 --demo
dotnet run --project src/Cli -- --lab07 --extras
```

`--lab07` без назви звіту запускає `report-top`; `--file`, `--cache` та
`--factory` з попередніх робіт залишаються доступними. За порожнього каталогу
списки показують «Немає записів», а підсумки повертають чотири нулі.
`SampleData` має 19 товарів, чотири одиниці виміру та три товари із
залишком менше 10. Топ-5 сортується за кількістю та унікальним Id, тому
повторні запуски дають той самий порядок. За базовими даними сума становить
752, середнє — 39,58, найбільший залишок — 90.

### Запити та відкладене виконання

| Метод | Питання | Оператори |
| --- | --- | --- |
| `LowStock` | Яких товарів менше порога? | `Where`, `OrderBy`, `ThenBy`, `Select`, `ToList` |
| `TopByQuantity` | Які N товарів мають найбільший залишок? | `TopBy`, `OrderByDescending`, `ThenBy`, `Take`, `Select` |
| `ByUnit` | Скільки позицій і одиниць у кожній групі? | `GroupBy`, `Count`, `Sum`, `OrderByDescending` |
| `Totals` | Які загальна, середня і максимальна кількості? | `ToList`, `Sum`, `Average`, `Max` |

Методне групування:

```csharp
source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
    .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
    .OrderByDescending(g => g.TotalQuantity)
    .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
    .ToList();
```

У демо джерелом є локальний список із `service.All().ToList()`, бо `All()`
повертає новий знімок. Побудова `source.Where(...)` ще не виконує запит:
перший `Count()` бачить 3 дефіцитні позиції, після `source.Add(...)` другий
`Count()` бачить 4. `ToList()` створює знімок; після `source.Clear()` у ньому
залишаються 4 записи. `Where` і `Select` працюють відкладено; `Count`, `Sum`
та `ToList` запускають перебір. `OrderBy` і `GroupBy` під час перебору
буферизують елементи, тому повторне виконання може бути дорогим.

### Додаткові завдання

1. `TopMovements` робить `Join` товарів і переміщень за `Product.Id` /
   `Movement.ProductId` та повертає DTO `MovementLine`.
2. Узагальнений метод-розширення `TopBy<T>` із другим ключем використано
   у двох звітах: `TopByQuantity` та `TopMovements`.
3. `ByUnitQuery` задає групування у синтаксисі `from/group/into/let/orderby/select`.
   Методний синтаксис компактніший для ланцюжка фільтрів і сортування;
   синтаксис запиту виразніше показує групу та її агрегат. На поточних
   даних обидва способи повертають однакові підсумки. Методна версія
   групує з `OrdinalIgnoreCase`, а версія запиту нормалізує ключ до нижнього
   регістру перед групуванням.
4. `--extras` вимірює `Stopwatch` на 100 000 згенерованих товарів: запит
   `Where().TopBy()` порівнюється з `Where().ToList().TopBy()`. Перед
   вимірюванням перевіряється тотожність результатів, виконується прогрів,
   а друкується медіана п'яти запусків. Проміжний список потребує
   додаткової пам'яті; час залежить від комп'ютера і стану процесу.

Звіт лабораторної №7 зберігається окремо від репозиторію.
