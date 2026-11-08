using EventBus.Base;

namespace EventBus.UsersEvents;

public sealed record UserBlockStatusChangedIntegration
(
    Guid UserId,
    bool IsBlocked
) : IntegrationEvent;