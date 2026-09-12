using System.Runtime.InteropServices;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool jsonMode = args.Contains("--json");

var info = new EnvironmentInfo(
    Title: "CrossApp – лабораторна з крос-платформного програмування",
    Student: "Студинський Артур, Група - ФЕІ-35",
    OsDescription: RuntimeInformation.OSDescription,
    OsEnvironment: Environment.OSVersion.ToString(),
    ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
    ClrVersion: Environment.Version.ToString(),
    Runtime: RuntimeInformation.FrameworkDescription,
    AppDirectory: AppContext.BaseDirectory,
    CurrentDirectory: Environment.CurrentDirectory,
    Domain: "Склад (товари, партії, залишки, переміщення)"
);

if (jsonMode)
{
    var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine(json);
}
else
{
    Console.WriteLine(info.Title);
    Console.WriteLine($"Студент: {info.Student}");
    Console.WriteLine(new string('.', 67));
    Console.WriteLine($"ОС (OSDescription)   : {info.OsDescription}");
    Console.WriteLine($"ОС (Environment)     : {info.OsEnvironment}");
    Console.WriteLine($"Архітектура процесу  : {info.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)    : {info.ClrVersion}");
    Console.WriteLine($"Runtime              : {info.Runtime}");
    Console.WriteLine($"Каталог застосунку   : {info.AppDirectory}");
    Console.WriteLine($"Поточний каталог     : {info.CurrentDirectory}");
    Console.WriteLine(new string('.', 67));
    Console.WriteLine($"Предметна область: {info.Domain}");
}

record EnvironmentInfo(
    string Title,
    string Student,
    string OsDescription,
    string OsEnvironment,
    string ProcessArchitecture,
    string ClrVersion,
    string Runtime,
    string AppDirectory,
    string CurrentDirectory,
    string Domain
);