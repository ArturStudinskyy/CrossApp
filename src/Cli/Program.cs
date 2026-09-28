using Core;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool jsonMode = args.Contains("--json");

// Program.cs містить лише виклик Core і форматування виводу[cite: 1].
EnvironmentReport report = EnvironmentInfo.Collect();

if (jsonMode)
{
    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    var json = JsonSerializer.Serialize(report, jsonOptions);
    Console.WriteLine(json);
}
else
{
    Console.WriteLine(report.Title);
    Console.WriteLine($"Студент: {report.Student}");
    Console.WriteLine(new string('.', 67));
    Console.WriteLine($"ОС (OSDescription)   : {report.OsDescription}");
    Console.WriteLine($"ОС (Environment)     : {report.OsEnvironment}");
    Console.WriteLine($"Архітектура процесу  : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (визначено)      : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET)       : {report.ReportedRid}");
    Console.WriteLine($"Версія .NET (CLR)    : {report.ClrVersion}");
    Console.WriteLine($"Runtime              : {report.FrameworkDescription}");
    Console.WriteLine($"Каталог застосунку   : {report.BaseDirectory}");
    Console.WriteLine($"Поточний каталог     : {report.CurrentDirectory}");
    Console.WriteLine($"Примітка збірки      : {report.BuildNote}");
    Console.WriteLine(new string('.', 67));
    Console.WriteLine($"Предметна область: {report.Domain}");
}