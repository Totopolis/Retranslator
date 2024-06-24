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
        string details,
        AttributesValue attributes) : base(id)
    {
        RequestId = requestId;
        Details = details;
        Debit = debit;
        Credit = credit;
        Attributes = attributes;
    }

    public ulong RequestId { get; private set; }

    public PartValue Debit { get; private set; }

    public PartValue Credit { get; private set; }

    public string Details { get; private set; }

    public AttributesValue Attributes { get; private set; }

    public Result<XmlDocument> ConvertToXmlDocument()
    {
        XmlWriterSettings settings = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "\t",
            OmitXmlDeclaration = true
        };

        try
        {
            using var ms = new MemoryStream();
            using var writer = XmlWriter.Create(ms, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("invoice_payment");
            writer.WriteElementString("id", RequestId.ToString());
            writer.WriteElementString("debit", Debit.AccountNumber);
            writer.WriteElementString("credit", Credit.AccountNumber);
            writer.WriteElementString("amount", Debit.Amount.ToString());
            writer.WriteElementString("currency", Debit.Currency);
            writer.WriteElementString("details", Details);

            if (Attributes.TryGetPackValue(out var packValue))
            {
                writer.WriteElementString("pack", packValue);
            }

            writer.WriteEndElement();
            writer.Flush();

            var doc = new XmlDocument();
            ms.Position = 0;
            doc.Load(ms);
            // doc.Save(Console.Out);

            // RequestId is idempotency key for external server
            return doc;
        }
        catch (Exception ex)
        {
            return Result.Failure<XmlDocument>(new Error(
                code: "Payment.ConvertToXmlDocument",
                message: ex.Message));
        }
    }

    public static Result<Payment> Create(JsonRequest.JsonRequest request)
    {
        // 1. Check json validity
        if (!IsJsonValid(request.Content, out var doc))
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_IsJsonValid);
        }

        // 2. Extract requestId
        if (!doc.RootElement.TryGetProperty("request", out var requestElement))
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_TryGetRequestProperty);
        }

        if (!requestElement.TryGetProperty("id", out var requestIdElement))
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_TryGetRequestIdProperty);
        }

        if (!requestIdElement.TryGetUInt64(out var requestIdValue))
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_TryGetRequestIdValue);
        }

        // 3. Extract details
        if (!doc.RootElement.TryGetProperty("details", out var detailsElement))
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_TryGetDetailsProperty);
        }

        var detailsValue = detailsElement.GetString();
        // TODO: need check buisness condition
        if (string.IsNullOrWhiteSpace(detailsValue))
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_TryGetDetailsValue);
        }

        // 4. Extract debit part
        if (!doc.RootElement.TryGetProperty("debitPart", out var debitPartElement))
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_TryGetDebitPartProperty);
        }

        var debitPart = PartValue.Create(debitPartElement);

        if (debitPart.IsFailure)
        {
            return Result.Failure<Payment>(debitPart.Error);
        }

        // 5. Extract credit part
        if (!doc.RootElement.TryGetProperty("creditPart", out var creditPartElement))
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_TryGetCreditPartProperty);
        }

        var creditPart = PartValue.Create(creditPartElement);

        if (creditPart.IsFailure)
        {
            return Result.Failure<Payment>(creditPart.Error);
        }

        // 6. Extract attributes
        if (!doc.RootElement.TryGetProperty("attributes", out var attributesElement))
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_TryGetAttributesProperty);
        }

        var attributes = AttributesValue.Create(attributesElement);
        if (attributes.IsFailure)
        {
            return Result.Failure<Payment>(attributes.Error);
        }

        // 7. Check debit credit consistency
        if (debitPart.Value.Amount != creditPart.Value.Amount)
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_AmountCheck);
        }

        if (debitPart.Value.Currency != creditPart.Value.Currency)
        {
            return Result.Failure<Payment>(PaymentErrors.Payment_Create_CurrencyCheck);
        }

        return new Payment(
            // TODO: use the same id as jsonRequest.Id
            new PaymentId(Guid.NewGuid()),
            requestIdValue,
            debitPart.Value,
            creditPart.Value,
            detailsValue,
            attributes.Value);
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
