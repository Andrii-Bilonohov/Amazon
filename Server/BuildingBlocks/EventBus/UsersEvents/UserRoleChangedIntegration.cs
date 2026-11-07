using EventBus.Base;

namespace EventBus.UsersEvents;

public record UserRoleChangedIntegration
(
    Guid UserId, 
    string Role
) : IntegrationEvent;