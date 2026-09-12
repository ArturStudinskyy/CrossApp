# CrossApp

Наскрізний проєкт з крос-платформного програмування.

**Предметна область:** Склад.

**Сутності:** Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення).

**Призначення:** облік залишків товарів по партіях на складах та фіксація переміщень товару.


## Запуск

\\dotnet build

\\dotnet run --project src/Cli

## Середовище

.NET SDK 10.0.302, RID: win-x64, OS: Windows 10.0.26200

Редактор: VS Code


### 1. Публікація self-contained під двома RID

Команди:

\\dotnet publish src/Cli -c Release -r win-x64   --self-contained true\\

\\dotnet publish src/Cli -c Release -r linux-x64 --self-contained true\\


Порівняння розміру каталогів publish:

| RID       | Розмір каталогу publish |
|-----------|-------------------------|
| win-x64   | 76,651177406311 МБ      |
| linux-x64 | 78,8115434646606 МБ     |

### 2. Прапорець --json

Перевірка:

\\dotnet run --project src/Cli

dotnet run --project src/Cli -- --json

