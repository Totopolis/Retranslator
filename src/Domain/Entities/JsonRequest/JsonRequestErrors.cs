using Domain.Shared;

namespace Domain.Entities.JsonRequest;

public static class JsonRequestErrors
{
    public static Error JsonRequest_CreateNew = new Error(
        code: "JsonRequest.CreateNew",
        message: "Json content is null or whitestaces");
}
