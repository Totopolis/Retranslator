using Application.EventConsumers;
using Domain;
using Domain.Abstractions;
using Domain.Entities.JsonRequest;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Application.Tests;

public class ExternalConsumerTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider;

    private readonly IJsonRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ITestHarness _harness;

    public ExternalConsumerTests()
    {
        _repository = Substitute.For<IJsonRequestRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _serviceProvider = new ServiceCollection()
            .AddSingleton(_unitOfWork)
            .AddSingleton(_repository)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<ExternalJsonEventConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _serviceProvider.GetRequiredService<ITestHarness>();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _serviceProvider.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _harness.Start();
    }

    [Fact]
    public async void ExternalConsumer_Should_CallInsertRepository_WhenCorrectRequest()
    {
        var client = _harness.GetRequestClient<ExternalRequestContract>();
        await _harness.Bus.Publish(new ExternalRequestContract()
        {
            JsonContent = DataSample.CorrectPaymentJson
        });
        await _harness.Consumed.Any();

        _repository.Received(1).Insert(Arg.Is<JsonRequest>(
            req => req.State == JsonRequestState.Received &&
            req.Content == DataSample.CorrectPaymentJson));
    }

    // TODO: catch ReceivedDomainEvent
}
