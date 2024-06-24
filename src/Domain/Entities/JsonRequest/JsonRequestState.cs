using Ardalis.SmartEnum;

namespace Domain.Entities.JsonRequest;

public sealed class JsonRequestState : SmartEnum<JsonRequestState>
{
    public static readonly JsonRequestState Received = new(nameof(Received), 1);
    public static readonly JsonRequestState Delivered = new(nameof(Delivered), 2);
    public static readonly JsonRequestState Failed = new(nameof(Failed), 3);

    private JsonRequestState(string name, int value) : base(name, value)
    {
    }
}
