using Domain.Entities.JsonRequest;
using Domain.Entities.Payment;
using Domain.Errors;

namespace Domain.Tests;

public class PaymentTests
{
    [Fact]
    public void Create_ReturnPayment_WhenCorrectJson()
    {
        var request = JsonRequest.CreateNew(TestDataSource.CorrectPaymentJson);

        var payment = Payment.Create(request.Value);

        Assert.True(payment.IsSuccess);
    }

    [Fact]
    public void CreatedPayment_Should_Return_Same_Amount()
    {
        var request = JsonRequest.CreateNew(TestDataSource.CorrectPaymentJson);

        var payment = Payment.Create(request.Value);
        var obj = payment.Value;
        
        Assert.True(obj.Debit.Amount == obj.Credit.Amount);
    }

    [Fact]
    public void CreatedPayment_Should_Return_Correct_Pack()
    {
        var request = JsonRequest.CreateNew(TestDataSource.CorrectPaymentJson);
        var payment = Payment.Create(request.Value);
        var obj = payment.Value;

        var packFinded = obj.Attributes.TryGetPackValue(out var packValue);

        Assert.True(packFinded);
        Assert.True(packValue == "37");
    }

    [Fact]
    public void Create_ReturnDebitNotFound_WhenIncorrectJson()
    {
        var request = JsonRequest.CreateNew(TestDataSource.PaymentJsonWithoutDebit);
        var payment = Payment.Create(request.Value);

        Assert.True(payment.IsFailure);
        Assert.Equal(payment.Error, DomainErrors.Payment.DebitPartPropertyNotFound);
    }
}
