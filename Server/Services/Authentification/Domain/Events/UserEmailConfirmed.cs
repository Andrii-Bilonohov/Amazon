using Domain.Enums;
using SharedKernel;

namespace Domain.Events;

public sealed record UserEmailConfirmed
(
    Guid UserId, 
    string Email, 
    Role Role
) : DomainEvent;