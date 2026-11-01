namespace EventBus.UsersEvents;

public record UserDeleted
(
    Guid UserId,
    bool IsDeleted,
    DateTime OccuredOn
);