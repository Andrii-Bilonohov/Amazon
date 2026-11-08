using EventBus.Base;

namespace EventBus.UsersEvents;

public record EmailConfirmationRequestedIntegration
(
    Guid UserId, 
    string Email, 
    string Code
) : IntegrationEvent;