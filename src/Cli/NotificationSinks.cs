using System.Text;
using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

internal static class NotificationText
{
    public static string Format(CatalogChangedEventArgs change) =>
        $"{change.At:HH:mm:ss} {change.Kind,-8} {change.Id} " +
        $"{change.Delta,4:+0;-0;0} -> залишок {change.QuantityAfter}";
}

internal sealed class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs change)
    {
        try
        {
            Console.WriteLine(NotificationText.Format(change));
        }
        catch (IOException)
        {
            // Закритий потік виводу не скасовує виконану складську операцію.
        }
    }
}

internal sealed class FileSink(string path) : INotificationSink
{
    private readonly string _path = Path.GetFullPath(path);

    public void Notify(CatalogChangedEventArgs change)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.AppendAllText(_path, NotificationText.Format(change) + Environment.NewLine,
                new UTF8Encoding(false));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Обробляємо помилку саме на стороні підписника, а не в Core.
            Console.Error.WriteLine($"Не вдалося записати журнал: {ex.Message}");
        }
    }
}

