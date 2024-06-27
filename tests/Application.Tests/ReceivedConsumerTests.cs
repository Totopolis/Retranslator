using Application.Abstractions;
using Application.EventConsumers;
using Domain;
using Domain.Abstractions;
using Domain.DomainEvents;
using Domain.Entities.JsonRequest;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Xml;

namespace Application.Tests;

public class ReceivedConsumerTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider;

    private readonly IJsonRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebhookSender _webhookSender;

    private readonly ITestHarness _harness;

    public ReceivedConsumerTests()
    {
        _repository = Substitute.For<IJsonRequestRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _webhookSender = Substitute.For<IWebhookSender>();

        _serviceProvider = new ServiceCollection()
            .AddSingleton(_unitOfWork)
            .AddSingleton(_repository)
            .AddSingleton(_webhookSender)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<ReceivedDomainEventConsumer>();
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
    public async void ReceivedRequest_Should_PostXml_WhenCorrectRequest()
    {
        var jsonRequest = JsonRequest.CreateNew(DataSample.CorrectPaymentJson).Value;
        _repository.GetById(jsonRequest.Id).Returns(jsonRequest);

        var client = _harness.GetRequestClient<JsonRequestReceivedDomainEvent>();
        await _harness.Bus.Publish(new JsonRequestReceivedDomainEvent(
            Id: jsonRequest.Id,
            Content: jsonRequest.Content,
            Received: jsonRequest.Received));
        await _harness.Consumed.Any();

        await _webhookSender.Received(1).PostXml(Arg.Any<XmlDocument>());
    }
}
