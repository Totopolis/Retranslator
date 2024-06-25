using Application;
using Application.Abstractions;

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
                new ExternalRequestContract() { JsonContent = _jsonBody },
                stoppingToken);

            _logger.LogInformation("Single json request generated");
        }

		while (!stoppingToken.IsCancellationRequested)
        {
			await Task.Delay(1000, stoppingToken);
		}
	}

    private const string _jsonBody = @"{
	""request"": {
		""id"": 27454821037510912,
		""document"": {
			""id"": 27454820926361856,
			""type"": ""INVOICE_PAYMENT""
		}
	},
	""debitPart"": {
		""agreementNumber"": ""RUS01"",
		""accountNumber"": ""30109810000000000001"",
		""amount"": 3442.79,
		""currency"": ""810"",
		""attributes"": {}
	},
	""creditPart"": {
		""agreementNumber"": ""RUS01"",
		""accountNumber"": ""30233810000000000001"",
		""amount"": 3442.79,
		""currency"": ""810"",
		""attributes"": {}
	},
	""details"": ""RASCHET"",
	""bankingDate"": ""2023-07-26"",
	""attributes"": {
		""attribute"": [
			{
				""code"": ""pack"",
				""attribute"": ""37""
			}
		]
	}
}";

}
