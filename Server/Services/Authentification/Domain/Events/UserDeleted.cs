using SharedKernel;

namespace Domain.Events;

public record UserDeleted
(
    Guid UserId
) : DomainEvent;