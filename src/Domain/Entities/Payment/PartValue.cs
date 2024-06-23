using Domain.Primitives;
using Domain.Shared;
using System.Text.Json;

namespace Domain.Entities.Payment;

public sealed class PartValue : ValueObject
{
    private PartValue(
        string agreementNumber,
        string accountNumber,
        double amount,
        string currency)
    {
        AggreementNumber = agreementNumber;
        AccountNumber = accountNumber;
        Amount = amount;
        Currency = currency;
    }

    public string AggreementNumber { get; private set; }

    public string AccountNumber { get; private set; }

    public double Amount { get; private set; }

    public string Currency { get; private set; }

    public static Result<PartValue> Create(JsonElement jsonElement)
    {
        // 1. agreementNumber
        if (!jsonElement.TryGetProperty("agreementNumber", out var agreementNumberElement))
        {
            return Result.Failure<PartValue>(new Error(
                code: "PartValue.Create.TryGetAgreementNumberProperty",
                message: "TryGetAgreementNumberProperty"));
        }

        var aggreementNumberValue = agreementNumberElement.GetString();
        if (string.IsNullOrWhiteSpace(aggreementNumberValue))
        {
            return Result.Failure<PartValue>(new Error(
                code: "PartValue.Create.TryGetAgreementNumberValue",
                message: "TryGetAgreementNumberValue"));
        }

        // 2. accountNumber
        if (!jsonElement.TryGetProperty("accountNumber", out var accountNumberElement))
        {
            return Result.Failure<PartValue>(new Error(
                code: "PartValue.Create.TryGetAccountNumberProperty",
                message: "TryGetAccountNumberProperty"));
        }

        var accountNumberValue = accountNumberElement.GetString();
        if (string.IsNullOrWhiteSpace(accountNumberValue))
        {
            return Result.Failure<PartValue>(new Error(
                code: "PartValue.Create.TryGetAccountNumberValue",
                message: "TryGetAccountNumberValue"));
        }

        // 3. amount
        if (!jsonElement.TryGetProperty("amount", out var amountElement))
        {
            return Result.Failure<PartValue>(new Error(
                code: "PartValue.Create.TryGetAmountProperty",
                message: "TryGetAmountProperty"));
        }

        if (!amountElement.TryGetDouble(out var amountValue) || amountValue < 0)
        {
            return Result.Failure<PartValue>(new Error(
                code: "PartValue.Create.TryGetAmountValue",
                message: "TryGetAmountValue"));
        }

        // 4. currency
        if (!jsonElement.TryGetProperty("currency", out var currencyElement))
        {
            return Result.Failure<PartValue>(new Error(
                code: "PartValue.Create.TryCurrencyProperty",
                message: "TryCurrencyProperty"));
        }

        var currencyValue = currencyElement.GetString();
        // TODO: need buisness rules check (810)
        if (string.IsNullOrWhiteSpace(currencyValue) || currencyValue != "810")
        {
            return Result.Failure<PartValue>(new Error(
                code: "PartValue.Create.TryCurrencyValue",
                message: "TryCurrencyValue"));
        }

        return new PartValue(
            aggreementNumberValue,
            accountNumberValue,
            amountValue,
            currencyValue);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return AggreementNumber;
        yield return AccountNumber;
        yield return Amount;
        yield return Currency;
    }
}
