using EventBus.Base;

namespace EventBus.UsersEvents;

public record PasswordResetRequestedIntegration
(
    Guid UserId,
    string EmaiL,
    string Code
) : IntegrationEvent;