using Application.UseCases;
using EventBus.UsersEvents;
using MassTransit;
namespace Worker.Consumers;

public sealed class EmailConfirmationRequestedConsumer(SendEmailConfirmation useCase)
    : IConsumer<EmailConfirmationRequestedIntegration>
{
    public Task Consume(ConsumeContext<EmailConfirmationRequestedIntegration> ctx)
    {
        var m = ctx.Message;
        return useCase.ExecuteAsync(m.EventId, m.UserId, m.Email, m.Code, m.OccurredOn, ctx.CancellationToken);
    }
}
