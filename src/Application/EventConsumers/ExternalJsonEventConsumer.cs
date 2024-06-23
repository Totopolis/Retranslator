using Domain.Abstractions;
using Domain.Entities.JsonRequest;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.EventConsumers;

// Receive raw json from external source
// TODO: remove masstransit dependency in application layer
public class ExternalJsonEventConsumer : IConsumer<Batch<string>>
{
    private readonly IJsonRequestRepository _repo;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<ExternalJsonEventConsumer> _logger;

    public ExternalJsonEventConsumer(
        IUnitOfWork unitOfWork,
        IJsonRequestRepository repo,
        ILogger<ExternalJsonEventConsumer> logger)
    {
        _unitOfWork = unitOfWork;
        _repo = repo;
        _logger = logger;
    }

    // TODO: use cancel-token
    public async Task Consume(ConsumeContext<Batch<string>> context)
    {
        _logger.LogInformation($"Received batch of json requests (size={context.Message.Length})");

        var batch = context.Message;

        if (batch.Length == 0)
        {
            return;
        }

        foreach (var item in batch)
        {
            var jsonRequest = JsonRequest.CreateNew(item.Message);
            if (jsonRequest.IsFailure)
            {
                _logger.LogCritical($"External source provide incorrect raw string (msgId={item.MessageId})");
                continue;
            }

            _repo.Insert(jsonRequest.Value);
        }

        // 2. Save to db (domain events will be raised)
        // TODO: try-catch-retry
        await _unitOfWork.SaveChangesAsync();
    }
}
