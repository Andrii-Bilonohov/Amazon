using Domain.Enums;
using SharedKernel;

namespace Domain.Events;

public sealed record UserRegistered
(
    Guid UserId, 
    string Email, 
    Role Role
) : DomainEvent;