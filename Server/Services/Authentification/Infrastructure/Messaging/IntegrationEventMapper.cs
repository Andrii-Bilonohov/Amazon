using Domain.Events;
using EventBus.Base;
using EventBus.UsersEvents;
using SharedKernel;

namespace Infrastructure.Messaging;

public static class IntegrationEventMapper
{
    public static IIntegrationEvent? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        UserEmailConfirmed e => new UserEmailConfirmedIntegration(e.UserId, e.Email, e.Role.ToString())
            { EventId = e.EventId, OccurredOn = e.OccurredOn },

        UserRoleChanged e => new UserRoleChangedIntegration(e.UserId, e.Role.ToString())
            { EventId = e.EventId, OccurredOn = e.OccurredOn },

        UserBlockStatusChanged e => new UserBlockStatusChangedIntegration(e.UserId, e.IsBlocked)
            { EventId = e.EventId, OccurredOn = e.OccurredOn },

        UserDeleted e => new UserDeletedIntegration(e.UserId)
            { EventId = e.EventId, OccurredOn = e.OccurredOn },

        PasswordResetRequested e => new PasswordResetRequestedIntegration(e.UserId, e.Email, e.Code)
            { EventId = e.EventId, OccurredOn = e.OccurredOn },

        EmailConfirmationRequested e => new EmailConfirmationRequestedIntegration(e.UserId, e.Email, e.Code)
            { EventId = e.EventId, OccurredOn = e.OccurredOn },

        _ => throw new InvalidOperationException(
            $"No integration mapping for domain event {domainEvent.GetType().Name}.")
    };
}