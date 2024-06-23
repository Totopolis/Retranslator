using Application.Abstractions;
using Domain.Abstractions;
using Domain.DomainEvents;
using Domain.Entities.JsonRequest;
using Domain.Entities.Payment;
using Domain.Shared;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Xml;

namespace Application.EventConsumers;

public class ReceivedDomainEventConsumer : IConsumer<JsonRequestReceivedDomainEvent>
{
    private readonly IJsonRequestRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebhookSender _webhookSender;
    private readonly ILogger<JsonRequestReceivedDomainEvent> _logger;

    public ReceivedDomainEventConsumer(
        IJsonRequestRepository repo,
        IUnitOfWork unitOfWork,
        IWebhookSender webhookSender,
        ILogger<JsonRequestReceivedDomainEvent> logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _webhookSender = webhookSender;
        _logger = logger;
    }

    // TODO: use cancel-token & enable open-telemetry traces
    public async Task Consume(ConsumeContext<JsonRequestReceivedDomainEvent> context)
    {
        // TODO: enreach with body-property
        _logger.LogInformation("Start processing request");

        // 1. Get json-request aggregate
        var jsonRequest = await _repo.GetById(context.Message.Id);

        if (jsonRequest is null)
        {
            _logger.LogCritical($"Json request not found (id={context.Message.Id})");
            return;
        }

        // 2. Try create invoice-payment entity from json content
        Result<Payment> payment = Payment.Create(jsonRequest);

        if (payment.IsFailure)
        {
            await Failed(jsonRequest, payment.Error);
            return;
        }

        // 3. Try 3Convert to XML
        Result<XmlDocument> xmlDocument = payment.Value.ConvertToXmlDocument();

        if (xmlDocument.IsFailure)
        {
            await Failed(jsonRequest, xmlDocument.Error);
            return;
        }

        // 4. Send to external server, using polly policy inside
        var webhookResult = Result.Success();
        try
        {
            // There may be repetitions (when unitOfWork crashes)
            await _webhookSender.PostXml(xmlDocument.Value);
        }
        catch (Exception ex)
        {
            webhookResult = Result.Failure(new Error(
                code: "Webhook.PostXml",
                message: ex.Message));
        }

        if (webhookResult.IsFailure)
        {
            await Failed(jsonRequest, webhookResult.Error);
            return;
        }

        // it is success
        _logger.LogInformation(
            $"Payment xml document successfully delivered (id={jsonRequest.Id})");
        jsonRequest.Delivered();

        // 5. Save to db (domain events will be raised)
        // TODO: try-catch-retry
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task Failed(JsonRequest request, Error error)
    {
        _logger.LogError(error.Message);

        request.Failed(error.Message);

        // Save to db (domain event will be raised)
        // TODO: try-catch-retry
        await _unitOfWork.SaveChangesAsync();
    }
}
