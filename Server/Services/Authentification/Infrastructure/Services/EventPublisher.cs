using Application.Abstractions;
using MassTransit;

namespace Infrastructure.Services;

public class EventPublisher(IPublishEndpoint bus) : IEventPublisher
{
    public Task PublishAsync<T>(T @event, CancellationToken ct) where T : class
        => bus.Publish(@event, ct);
}