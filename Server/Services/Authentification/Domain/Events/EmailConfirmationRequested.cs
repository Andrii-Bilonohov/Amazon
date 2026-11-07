using SharedKernel;

namespace Domain.Events;

public record EmailConfirmationRequested
(
    Guid UserId,
    string Email,
    string PlainCode
) : DomainEvent;