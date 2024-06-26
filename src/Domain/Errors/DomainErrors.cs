﻿using Domain.Shared;

namespace Domain.Errors;

public static class DomainErrors
{
    public static class JsonRequest
    {
        public static Error IncorrectJsonString = new Error(
            code: "JsonRequest.CreateNew",
            message: "Json content is null or whitestaces");
    }

    public static class Payment
    {
        public static Error Create_IsJsonValid = new Error(
            code: "Payment.Create.IsJsonValid",
            message: "Json request content is not valide json object");

        public static Error Create_TryGetRequestProperty = new Error(
            code: "Payment.Create.TryGetRequestProperty",
            message: "Request property not found");

        public static Error Create_TryGetRequestIdProperty = new Error(
            code: "Payment.Create.TryGetRequestIdProperty",
            message: "Request id property not found");

        public static Error Create_TryGetRequestIdValue = new Error(
            code: "Payment.Create.TryGetRequestIdValue",
            message: "Bad request id value");

        public static Error Create_TryGetDetailsProperty = new Error(
            code: "Payment.Create.TryGetDetailsProperty",
            message: "Request details property not found");

        public static Error Create_TryGetDetailsValue = new Error(
            code: "Payment.Create.TryGetDetailsValue",
            message: "Bad request details value");

        public static Error DebitPartPropertyNotFound = new Error(
            code: "Payment.DebitPartPropertyNotFound",
            message: "Request debit part property not found");

        //
        public static Error Create_TryGetCreditPartProperty = new Error(
            code: "Payment.Create.TryGetCreditPartProperty",
            message: "Request credit part property not found");

        //
        public static Error Create_TryGetAttributesProperty = new Error(
            code: "Payment.Create.TryGetAttributesProperty",
            message: "Request attributes property not found");

        public static Error Create_AmountCheck = new Error(
            code: "Payment.Create.AmountCheck",
            message: "Debit amount must be equals credit amount");

        public static Error Create_CurrencyCheck = new Error(
            code: "Payment.Create.CurrencyCheck",
            message: "Debit currency must be same as credit currency");
    }

    public static class AttributesValue
    {
        public static Error Create = new Error(
            code: "AttributesValue.Create",
            message: "Attribute property must be an array kind");

        public static Error Create_IterateArray_TryGetCodeProperty = new Error(
            code: "AttributesValue.Create.IterateArray.TryGetCodeProperty",
            message: "Array item must constains code property");

        public static Error Create_IterateArray_CheckCodeValue = new Error(
            code: "AttributesValue.Create.IterateArray.CheckCodeValue",
            message: "Code value can not be null or whitespace");

        public static Error Create_IterateArray_CheckMapContainsCode = new Error(
            code: "AttributesValue.Create.IterateArray.CheckMapContainsCode",
            message: "The code must not be repeated in the array");

        public static Error Create_IterateArray_TryGetAttributeValueProperty = new Error(
            code: "AttributesValue.Create.IterateArray.TryGetAttributeValueProperty",
            message: "Array item must constains attribute value property");

        public static Error Create_IterateArray_CheckAttributeValue = new Error(
            code: "AttributesValue.Create.IterateArray.CheckAttributeValue",
            message: "Attribute value can not be null or whitespace");
    }
}
