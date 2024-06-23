using Domain.Primitives;
using Domain.Shared;
using System.Text.Json;
using System.Xml;

namespace Domain.Entities.Payment;

public sealed class Payment : Entity<PaymentId>
{
    private Payment(
        PaymentId id,
        ulong requestId,
        PartValue debit,
        PartValue credit,
        string details) : base(id)
    {
        RequestId = requestId;
        Details = details;
        Debit = debit;
        Credit = credit;
    }

    public ulong RequestId { get; private set; }

    public PartValue Debit { get; private set; }

    public PartValue Credit { get; private set; }

    public string Details { get; private set; }

    public Result<XmlDocument> ConvertToXmlDocument()
    {
        // Id is idempotency key for external server
        return new XmlDocument();
    }

    public static Result<Payment> Create(JsonRequest.JsonRequest request)
    {
        // 1. Check json validity
        if (!IsJsonValid(request.Content, out var doc))
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create.IsJsonValid",
                message: "Json request content is not valide json object"));
        }

        // 2. Extract requestId
        if (!doc.RootElement.TryGetProperty("request", out var requestElement))
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create.TryGetRequestProperty",
                message: "Request property not found"));
        }

        if (!requestElement.TryGetProperty("id", out var requestIdElement))
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create.TryGetRequestIdProperty",
                message: "Request id property not found"));
        }

        if (!requestIdElement.TryGetUInt64(out var requestIdValue))
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create.TryGetRequestIdValue",
                message: "Bad request id value"));
        }

        // 3. Extract details
        if (!doc.RootElement.TryGetProperty("details", out var detailsElement))
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create.TryGetDetailsProperty",
                message: "Request details property not found"));
        }

        var detailsValue = detailsElement.GetString();
        // TODO: need check buisness condition
        if (string.IsNullOrWhiteSpace(detailsValue))
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create.TryGetDetailsValue",
                message: "Bad request details value"));
        }

        // 4. Extract debit part
        if (!doc.RootElement.TryGetProperty("debitPart", out var debitPartElement))
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create.TryGetDebitPartProperty",
                message: "Request debit part property not found"));
        }

        var debitPart = PartValue.Create(debitPartElement);

        if (debitPart.IsFailure)
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create." + debitPart.Error.Code,
                message: debitPart.Error.Message));
        }

        // 5. Extract credit part
        if (!doc.RootElement.TryGetProperty("creditPart", out var creditPartElement))
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create.TryGetCreditPartProperty",
                message: "Request credit part property not found"));
        }

        var creditPart = PartValue.Create(creditPartElement);

        if (creditPart.IsFailure)
        {
            return Result.Failure<Payment>(new Error(
                code: "Payment.Create." + creditPart.Error.Code,
                message: creditPart.Error.Message));
        }

        return new Payment(
            new PaymentId(Guid.NewGuid()),
            requestIdValue,
            debitPart.Value,
            creditPart.Value,
            detailsValue);
    }

    private static bool IsJsonValid(string jsonContent, out JsonDocument doc)
    {
        doc = default!;

        try
        {
            doc = JsonDocument.Parse(jsonContent);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
