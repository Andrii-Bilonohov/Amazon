namespace EventBus.UsersEvents;

public record UserChangedBlockStatus
(
    Guid UserId,
    bool IsBlocked,
    DateTime OccuredOn
);