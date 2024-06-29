using Application.Abstractions;
using Domain.Abstractions;
using MassTransit;
using MassTransit.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persistence.MessageBroker;
using Persistence.Repositories;
using Persistence.Webhook;
using Polly;
using Polly.Extensions.Http;

namespace Persistence;

public static class ServiceExtensions
{
    public static IServiceCollection AddPersistenceOptions(this IServiceCollection services)
    {
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

        services
            .AddOptions<RabbitSettings>()
            .BindConfiguration(RabbitSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

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

        return services;
    }

    public static IServiceCollection AddMasstransitLocal(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            // DANGER: Auto registrations DONT WORK with INTERNALs consumers!
            // https://github.com/MassTransit/MassTransit/issues/2253
            // busConfigurator.AddConsumers(typeof(IEventBus).Assembly);

            // Handmage autoregistration
            var consumers = GetAllConsumersFromAssemblyContainsType<IEventBus>();
            busConfigurator.AddConsumers(consumers);

            busConfigurator.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection AddMasstransitHost(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            var consumers = GetAllConsumersFromAssemblyContainsType<IEventBus>();
            busConfigurator.AddConsumers(consumers);

            busConfigurator.UsingRabbitMq((ctx, rabbitConfigurator) =>
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

                rabbitConfigurator.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }

    public static IServiceCollection Replace<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> implementationFactory,
        ServiceLifetime lifetime)
        where TService : class
    {
        var descriptorToRemove = services.FirstOrDefault(d => d.ServiceType == typeof(TService));

        if (descriptorToRemove is null)
        {
            ArgumentNullException.ThrowIfNull(descriptorToRemove);
        }

        services.Remove(descriptorToRemove);

        var descriptorToAdd = new ServiceDescriptor(typeof(TService), implementationFactory, lifetime);

        services.Add(descriptorToAdd);

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

    private static Type[] GetAllConsumersFromAssemblyContainsType<T>()
    {
        var types = typeof(T).Assembly
            .GetTypes()
            .Where(RegistrationMetadata.IsConsumerOrDefinition)
            .ToArray();
        
        return types;
    }
}
