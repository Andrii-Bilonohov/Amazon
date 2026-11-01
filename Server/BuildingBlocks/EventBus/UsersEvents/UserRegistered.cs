namespace EventBus.UsersEvents;

public record UserRegistered
(
    Guid UserId,
    string Email,
    string Role,
    DateTime OccurredOn
);