using Domain.DomainEvents;
using Domain.Entities.JsonRequest;
using Domain.Errors;

namespace Domain.Tests;

public class JsonRequestTests
{
    [Fact]
    public void Create_ReturnObject_WhenCorrectJson()
    {
        var request = JsonRequest.CreateNew(DataSample.CorrectPaymentJson);

        Assert.True(request.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_ReturnFail_WhenIncorrectJson(string jsonContent)
    {
        var request = JsonRequest.CreateNew(jsonContent);

        Assert.True(request.IsFailure);
        Assert.Equal(request.Error, DomainErrors.JsonRequest.IncorrectJsonString);
    }

    [Fact]
    public void Create_Should_RaiseDomainEvent()
    {
        var request = JsonRequest.CreateNew(DataSample.CorrectPaymentJson);

        Assert.NotEmpty(
            request.Value.GetDomainEvents().OfType<JsonRequestReceivedDomainEvent>());
    }

    [Fact]
    public void Deliver_Should_RaiseDomainEvent()
    {
        var request = JsonRequest.CreateNew(DataSample.CorrectPaymentJson);

        request.Value.Delivered();

        Assert.NotEmpty(
            request.Value.GetDomainEvents().OfType<JsonRequestDeliveredDomainEvent>());
    }

    [Fact]
    public void Fail_Should_RaiseDomainEvent()
    {
        var request = JsonRequest.CreateNew(DataSample.CorrectPaymentJson);

        request.Value.Failed("Some error");

        Assert.NotEmpty(
            request.Value.GetDomainEvents().OfType<JsonRequestFailedDomainEvent>());
    }
}
