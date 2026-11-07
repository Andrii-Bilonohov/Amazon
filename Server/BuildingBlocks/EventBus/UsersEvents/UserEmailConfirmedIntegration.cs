using EventBus.Base;

namespace EventBus.UsersEvents;

public sealed record UserEmailConfirmedIntegration
(
    Guid UserId,
    string Email,
    string Role
) : IntegrationEvent;