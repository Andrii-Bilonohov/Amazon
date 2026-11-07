using SharedKernel;

namespace Domain.Events;

public record UserEmailConfirmed
(
    Guid UserId
 ) : DomainEvent;