using SharedKernel;

namespace Domain.Events;

public sealed record PasswordResetRequested
(
    Guid UserId, 
    string Email, 
    string Code
) : DomainEvent;
