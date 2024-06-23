using Domain.Primitives;
using Domain.Shared;
using System.Xml;

namespace Domain.Entities.Payment;

public sealed class Payment : Entity<PaymentId>
{
    private Payment(PaymentId id) : base(id)
    {
    }

    public Result<XmlDocument> ConvertToXmlDocument()
    {
        // Id is idempotency key for external server
        return new XmlDocument();
    }

    public static Result<Payment> Create(JsonRequest.JsonRequest request)
    {
        return default!;
    }
}
