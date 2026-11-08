using Application.UseCases;
using EventBus.UsersEvents;
using MassTransit;

namespace Worker.Consumers;

public sealed class PasswordResetRequestedConsumer(SendPasswordReset useCase)
    : IConsumer<PasswordResetRequestedIntegration>
{
    public Task Consume(ConsumeContext<PasswordResetRequestedIntegration> ctx)
    {
        var m = ctx.Message;
        return useCase.ExecuteAsync(m.EventId, m.UserId, m.EmaiL, m.Code, m.OccurredOn, ctx.CancellationToken);
    }
}
