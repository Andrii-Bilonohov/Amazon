using EventBus.Base;

namespace EventBus.UsersEvents;

public record UserRegisteredIntegration
(
    Guid UserId,
    string Email,
    string Role
) : IntegrationEvent;