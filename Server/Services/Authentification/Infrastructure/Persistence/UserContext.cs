using Domain.Entities;
using Infrastructure.Configurations;
using Infrastructure.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.Persistence;

public class UserContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    
    public UserContext(DbContextOptions<UserContext> options, IServiceProvider serviceProvider)
        : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new UserConfiguration());
        builder.AddInboxStateEntity();
        builder.AddTransactionalOutboxEntities();
        builder.Entity<User>().Ignore(u => u.DomainEvents);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var publishEndpoint = _serviceProvider.GetRequiredService<IPublishEndpoint>();
        foreach (var domainEvent in aggregates.SelectMany(a => a.DomainEvents))
        {
            var integrationEvent = IntegrationEventMapper.Map(domainEvent);
            if (integrationEvent is not null)
                await publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), ct);
        }

        var result = await base.SaveChangesAsync(ct); 
        aggregates.ForEach(a => a.ClearDomainEvents());
        return result;
    }
}