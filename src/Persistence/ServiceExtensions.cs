using Application.Abstractions;
using Domain.Abstractions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.MessageBroker;
using Persistence.Repositories;
using Persistence.Webhook;
using Polly;
using Polly.Extensions.Http;

namespace Persistence;

public static class ServiceExtensions
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddScoped<IEventBus, EventBus>();
        services.AddTransient<IWebhookSender, WebhookSender>();

        AddHttpAndPolly(services, config);

        // It is scoped service
        services.AddDbContext<RetranslatorDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJsonRequestRepository, JsonRequestRepository>();

        services
            .AddOptions<WebhookSettings>()
            .BindConfiguration(WebhookSettings.SectionName)
            .Validate(section =>
            {
                if (!Uri.IsWellFormedUriString(section.DestinationUri, UriKind.Absolute))
                {
                    return false;
                }

                return true;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<PostgreSettings>()
            .BindConfiguration(PostgreSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddMasstransitServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddOptions<RabbitSettings>()
            .BindConfiguration(RabbitSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            // Auto registrations
            busConfigurator.AddConsumers(typeof(IEventBus).Assembly);

            busConfigurator.AddDelayedMessageScheduler();

            busConfigurator.UsingInMemory((context, cfg) =>
            {
                cfg.UseDelayedMessageScheduler();
                cfg.ConfigureEndpoints(context);
            });

            // TODO: use RabbitMq with settings
            /*busConfigurator.UsingRabbitMq((ctx, rabbitConfigurator) =>
            {
                var settings = ctx
                    .GetRequiredService<IOptions<RabbitSettings>>()
                    .Value;

                rabbitConfigurator.Host(
                    new Uri(settings.Host),
                    h =>
                    {
                        h.Username(settings.UserName);
                        h.Password(settings.Password);
                    });
            });*/
        });

        return services;
    }

    private static void AddHttpAndPolly(
        IServiceCollection services,
        IConfiguration config)
    {
        IAsyncPolicy<HttpResponseMessage> policy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        services
            .AddHttpClient(PersistenceConstants.WebhookHttpCLientName)
            .AddPolicyHandler(policy);
    }
}
