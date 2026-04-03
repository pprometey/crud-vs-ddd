using Mediator;

namespace DoctorBooking.DDD.Application.Tests.Fakes;

/// <summary>
/// Fake publisher for testing that collects published events
/// </summary>
public sealed class FakePublisher : IPublisher
{
    private readonly List<object> _publishedEvents = [];

    public IReadOnlyList<object> PublishedEvents => _publishedEvents.AsReadOnly();

    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        _publishedEvents.Add(notification!);
        return ValueTask.CompletedTask;
    }

    public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
    {
        _publishedEvents.Add(notification);
        return ValueTask.CompletedTask;
    }

    public void Clear()
    {
        _publishedEvents.Clear();
    }
}
