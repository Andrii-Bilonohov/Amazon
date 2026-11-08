using Application.Abstractions;
using Application.Options;
using Application.UseCases;
using Infrastructure.Email;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Worker.Consumers;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host),
                "Email:Host is required (Email__Host).")
            .Validate(o => o.Port is > 0 and <= 65535,
                "Email:Port must be 1-65535 (Email__Port).")
            .Validate(o => !string.IsNullOrWhiteSpace(o.User) && o.User.Contains('@'),
                "Email:User must be a full email address (Email__User).")
            .Validate(o => o.Password.Length == 16 && !o.Password.Contains(' '),
                "Email:Password must be a 16-character app password without spaces (Email__Password).")
            .Validate(o => !string.IsNullOrWhiteSpace(o.FromName),
                "Email:FromName must not be empty.")
            .ValidateOnStart();
        
        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        

        services.AddSingleton<IEmailSender, EmailSender>();
        
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddSingleton(TimeProvider.System);
        
        services.AddScoped<SendEmailConfirmation>();
        services.AddScoped<SendPasswordReset>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<EmailConfirmationRequestedConsumer>();
            x.AddConsumer<PasswordResetRequestedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username(configuration["RABBITMQ_USER"]!);
                    h.Password(configuration["RABBITMQ_PASSWORD"]!);
                });
                cfg.ConfigureEndpoints(ctx);
            });
        });
        
        return services;
    }
}