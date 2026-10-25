ДОДАТКОВІ ЗАВДАННЯ — ЛАБОРАТОРНА №5

1. Завантаж ZIP і розпакуй його у тимчасову папку.
2. У корені D:\LNY\3.1\CPP виконай:
   Copy-Item "$env:USERPROFILE\Downloads\lab05_extras_pkg\Install-Lab05Extras.ps1" .
   Або безпосередньо запуск через шлях до розпакованого файлу.
3. Перебуваючи у корені CrossApp, запусти:
   & "<шлях до розпакованого архіву>\Install-Lab05Extras.ps1"
4. dotnet build
5. dotnet run --project src/Cli -- --lab05 --extras --cache --factory
6. Запусти з --file для файлового режиму.

Скрипт додає 3 файли, оновлює CatalogService, Program.cs та README.md.
Створює резервні копії змінених .cs файлів із розширенням .bak.
Після перевірки видали резервні копії перед git add . або додай лише потрібні файли.
