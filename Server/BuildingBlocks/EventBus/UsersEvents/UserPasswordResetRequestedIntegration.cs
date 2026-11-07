using EventBus.Base;

namespace EventBus.UsersEvents;

public record UserPasswordResetRequestedIntegration
(
    Guid UserId,
    string EmaiL,
    string Code
) : IntegrationEvent;