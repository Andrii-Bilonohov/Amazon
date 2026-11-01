using System.Text;
using Application.Abstractions;
using Application.Services;
using Domain.Abstractions;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString);
        });
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:ISSUER"],
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:AUDIENCE"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JWT:SECRET"]!)),
                    RoleClaimType = "role",
                    NameClaimType = "sub",
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
        
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<UserContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

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
        
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.AddScoped<IEventPublisher, EventPublisher>();
        
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IAuthentificationService, AuthentificationService>();
        services.AddScoped<ITokenService, TokenService>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
    
    // public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    // {
    //     app.UseMiddleware<ListenToOnlyGatewayApiMiddleware>();
    //     return app;
    // }
}