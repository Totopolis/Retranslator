namespace Domain.Entities.JsonRequest;

public enum JsonRequestState : byte
{
    Received = 1,
    Delivered = 2,
    Failed = 3
}
