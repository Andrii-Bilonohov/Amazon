using Domain.Enums;
using SharedKernel;

namespace Domain.Events;

public sealed record UserRoleChanged
(
    Guid UserId,
    Role Role
) : DomainEvent;