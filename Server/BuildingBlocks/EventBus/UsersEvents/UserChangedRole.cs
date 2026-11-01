namespace EventBus.UsersEvents;

public record UserRoleChanged
(
    Guid UserId, 
    string Role, 
    DateTime OccurredAt
);