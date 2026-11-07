using SharedKernel;

namespace Domain.Events;

public sealed record UserBlockStatusChanged
(
    Guid UserId, 
    bool IsBlocked
) : DomainEvent;