namespace EventBus.UsersEvents;

public record UserRequestedPasswordResetEvent
(
    Guid UserId,
    string EmaiL,
    string Code,
    DateTime OccurredOn
);