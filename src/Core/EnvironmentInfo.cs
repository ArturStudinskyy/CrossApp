using System.Runtime.InteropServices;
using System;

namespace Core;

// Record лише зберігає значення, які Cli потім друкує.
public sealed record EnvironmentReport(
    string Title,
    string Student,
    string OsDescription,
    string OsEnvironment,
    string ProcessArchitecture,
    string DetectedRid,
    string ReportedRid,
    string ClrVersion,
    string FrameworkDescription,
    string BaseDirectory,
    string CurrentDirectory,
    string Domain,
    string BuildNote
);

public static class EnvironmentInfo
{
    // Умовна компіляція для перевірки multi-targeting (Додаткове завдання).
#if NET10_0_OR_GREATER
    const string BuildNote = "збірка під net10.0";
#else
    const string BuildNote = "збірка під net8.0";
#endif

    // Метод Collect() містить алгоритм збору інформації, тому знаходиться в static class.
    public static EnvironmentReport Collect() => new(
        Title: "CrossApp – лабораторна з крос-платформного програмування",
        Student: "Студинський Артур, Група - ФЕІ-35",
        OsDescription: RuntimeInformation.OSDescription,
        OsEnvironment: Environment.OSVersion.ToString(),
        ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
        DetectedRid: DetectRid(),
        ReportedRid: RuntimeInformation.RuntimeIdentifier,
        ClrVersion: Environment.Version.ToString(),
        FrameworkDescription: RuntimeInformation.FrameworkDescription,
        BaseDirectory: AppContext.BaseDirectory,
        CurrentDirectory: Environment.CurrentDirectory,
        Domain: "Склад (товари, партії, залишки, переміщення)",
        BuildNote: BuildNote
    );

    // Ручне визначення RID з використанням тернарного оператора та switch-виразу[cite: 1].
    private static string DetectRid()
    {
        string os =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "unknown";

        string arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown" // Гілка за замовчуванням[cite: 1]
        };

        return $"{os}-{arch}";
    }
}