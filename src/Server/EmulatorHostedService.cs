using Application;
using Application.Abstractions;
using Domain;

namespace Server;

public class EmulatorHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmulatorHostedService> _logger;

    public EmulatorHostedService(
        IServiceProvider serviceProvider,
        ILogger<EmulatorHostedService> logger)
    {
		_serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmulatorHostedService started");

        return base.StartAsync(cancellationToken);
    }

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// Emulate single external request
		{
			await using var scope = _serviceProvider.CreateAsyncScope();

            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            await eventBus.PublishAsync(
                new ExternalRequestContract() { JsonContent = DataSample.CorrectPaymentJson },
                stoppingToken);

            _logger.LogInformation("Single json request generated");
        }

		while (!stoppingToken.IsCancellationRequested)
        {
			await Task.Delay(1000, stoppingToken);
		}
	}
}
