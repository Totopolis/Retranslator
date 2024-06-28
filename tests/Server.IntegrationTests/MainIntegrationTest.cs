using Application;
using Application.Abstractions;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;
using Persistence.Repositories;
using Testcontainers.PostgreSql;

namespace Server.IntegrationTests;

public class MainIntegrationTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("retranslator")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    [Fact]
    public async Task Test1()
    {
        var builder = Host.CreateEmptyApplicationBuilder(null);

        builder.Services.Configure<PostgreSettings>(
            x => new PostgreSettings() { ConnectionString = _dbContainer.GetConnectionString() });

        builder.Services
            .AddPersistenceServices(builder.Configuration)
            .AddMasstransitServices(builder.Configuration);
        
        var app = builder.Build();

        var cts = new CancellationTokenSource(10_000);
        var awaiter = app.RunAsync(cts.Token);

        var eventBus = app.Services.GetRequiredService<IEventBus>();
        await eventBus.PublishAsync(
                new ExternalRequestContract() { JsonContent = DataSample.CorrectPaymentJson },
                cts.Token);

        cts.Cancel();
        await awaiter;
    }
}
