using Infrastructure;
using DotNetEnv;

Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddNotificationInfrastructure(builder.Configuration);

builder.Build().Run();
