using Domain.Shared;

namespace Domain.Entities.Payment;

public static class PaymentErrors
{
    public static Error Payment_Create_IsJsonValid = new Error(
        code: "Payment.Create.IsJsonValid",
        message: "Json request content is not valide json object");

    public static Error Payment_Create_TryGetRequestProperty = new Error(
        code: "Payment.Create.TryGetRequestProperty",
        message: "Request property not found");

    public static Error Payment_Create_TryGetRequestIdProperty = new Error(
        code: "Payment.Create.TryGetRequestIdProperty",
        message: "Request id property not found");

    public static Error Payment_Create_TryGetRequestIdValue = new Error(
        code: "Payment.Create.TryGetRequestIdValue",
        message: "Bad request id value");

    public static Error Payment_Create_TryGetDetailsProperty = new Error(
        code: "Payment.Create.TryGetDetailsProperty",
        message: "Request details property not found");

    public static Error Payment_Create_TryGetDetailsValue = new Error(
        code: "Payment.Create.TryGetDetailsValue",
        message: "Bad request details value");

    public static Error Payment_Create_TryGetDebitPartProperty = new Error(
        code: "Payment.Create.TryGetDebitPartProperty",
        message: "Request debit part property not found");

    //
    public static Error Payment_Create_TryGetCreditPartProperty = new Error(
        code: "Payment.Create.TryGetCreditPartProperty",
        message: "Request credit part property not found");

    //
    public static Error Payment_Create_TryGetAttributesProperty = new Error(
        code: "Payment.Create.TryGetAttributesProperty",
        message: "Request attributes property not found");

    public static Error Payment_Create_AmountCheck = new Error(
        code: "Payment.Create.AmountCheck",
        message: "Debit amount must be equals credit amount");

    public static Error Payment_Create_CurrencyCheck = new Error(
        code: "Payment.Create.CurrencyCheck",
        message: "Debit currency must be same as credit currency");

    //
    //
    public static Error AttributesValue_Create = new Error(
        code: "AttributesValue.Create",
        message: "Attribute property must be an array kind");

    public static Error AttributesValue_Create_IterateArray_TryGetCodeProperty = new Error(
        code: "AttributesValue.Create.IterateArray.TryGetCodeProperty",
        message: "Array item must constains code property");

    public static Error AttributesValue_Create_IterateArray_CheckCodeValue = new Error(
        code: "AttributesValue.Create.IterateArray.CheckCodeValue",
        message: "Code value can not be null or whitespace");

    public static Error AttributesValue_Create_IterateArray_CheckMapContainsCode = new Error(
        code: "AttributesValue.Create.IterateArray.CheckMapContainsCode",
        message: "The code must not be repeated in the array");

    public static Error AttributesValue_Create_IterateArray_TryGetAttributeValueProperty = new Error(
        code: "AttributesValue.Create.IterateArray.TryGetAttributeValueProperty",
        message: "Array item must constains attribute value property");

    public static Error AttributesValue_Create_IterateArray_CheckAttributeValue = new Error(
        code: "AttributesValue.Create.IterateArray.CheckAttributeValue",
        message: "Attribute value can not be null or whitespace");

}
