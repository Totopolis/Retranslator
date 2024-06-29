using Application;
using Application.Abstractions;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;
using Persistence.MessageBroker;
using Persistence.Repositories;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Server.IntegrationTests;

public class MainIntegrationTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("retranslator")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:latest")
        .WithUsername("rabbit")
        .WithPassword("rabbit")
        .Build();

    private const int TestTimeoutMs = 60_000;
    private readonly CancellationTokenSource _timeoutTokenSource = new(TestTimeoutMs);
    private readonly CancellationTokenSource _resetTokenSource = new();

    private IHost _host = default!;

    public async Task DisposeAsync()
    {
        _host.Dispose();

        await _rabbitContainer.StopAsync();
        await _dbContainer.StopAsync();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitContainer.StartAsync();

        _host = await StartApp();
    }

    [Fact]
    public async Task WebhookReceived_WhenCorrectRequestInjected()
    {
        // TODO: find container whos slowly
        await Task.Delay(1_000);

        var eventBus = _host.Services.GetRequiredService<IEventBus>();
        await eventBus.PublishAsync(
                new ExternalRequestContract() { JsonContent = DataSample.CorrectPaymentJson },
                _timeoutTokenSource.Token);

        var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _timeoutTokenSource.Token,
            _resetTokenSource.Token);

        try
        {
            await Task.Delay(TestTimeoutMs, combinedTokenSource.Token);
        }
        catch(TaskCanceledException)
        {
        }

        if (!_resetTokenSource.IsCancellationRequested)
        {
            Assert.Fail("Webhook not raised. Timeout expired.");
        }
    }

    private async Task<Microsoft.Extensions.Hosting.IHost> StartApp()
    {
        var builder = Host.CreateEmptyApplicationBuilder(null);

        var pgConnectionString = _dbContainer.GetConnectionString();
        builder.Services.Configure<PostgreSettings>(
            x => x.ConnectionString = pgConnectionString);

        builder.Services.Configure<RabbitSettings>(x =>
        {
            x.Host = "rabbitmq://" +
                _rabbitContainer.Hostname +
                ":" +
                _rabbitContainer.GetMappedPublicPort(5672);

            x.UserName = "rabbit";
            x.Password = "rabbit";
        });

        builder.Services
            .AddPersistenceServices(builder.Configuration)
            .AddMasstransitHost(builder.Configuration);

        builder.Services.Replace<IWebhookSender>(
            sp => new AutoresetWebhookSender(_resetTokenSource), ServiceLifetime.Transient);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RetranslatorDbContext>();
            await dbContext.EnsureDatabaseStructureCreated(_timeoutTokenSource.Token);
        }

        // TODO: apply awaiter?
        var awaiter = app.StartAsync(_timeoutTokenSource.Token);

        return app;
    }
}
