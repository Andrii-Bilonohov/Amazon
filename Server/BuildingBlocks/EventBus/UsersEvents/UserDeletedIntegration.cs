using EventBus.Base;

namespace EventBus.UsersEvents;

public record UserDeletedIntegration
(
    Guid UserId
) : IntegrationEvent;