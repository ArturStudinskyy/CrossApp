using Core.Events;

namespace Core.Abstractions;

// Альтернатива події для порівняння ручного DI та Observer.
public interface INotificationSink
{
    void Notify(CatalogChangedEventArgs change);
}
