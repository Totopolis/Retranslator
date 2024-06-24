using Domain.Primitives;
using Domain.Shared;
using System.Text.Json;

namespace Domain.Entities.Payment;

// If u need postgres persistence - use array column
public sealed class AttributesValue : ValueObject
{
    public const string PackCode = "pack";

    // list of (code, attribute) pairs with uniq code constrains
    private readonly Dictionary<string, string> _values;

    private AttributesValue(Dictionary<string, string> values = default!)
    {
        _values = values ?? new();
    }

    public bool TryGetPackValue(out string value)
    {
        if (!_values.TryGetValue(PackCode, out var findedValue))
        {
            value = string.Empty;
            return false;
        }

        value = findedValue;
        return true;
    }

    public static Result<AttributesValue> Create(JsonElement jsonElement)
    {
        if (!jsonElement.TryGetProperty("attribute", out var attributeProperty))
        {
            // just skip, attribute property is optional
            return new AttributesValue();
        }

        if (attributeProperty.ValueKind != JsonValueKind.Array)
        {
            return Result.Failure<AttributesValue>(new Error(
                code: "AttributesValue.Create",
                message: "Attribute property must be an array kind"));
        }

        Dictionary<string, string> map = new();

        foreach (JsonElement item in attributeProperty.EnumerateArray())
        {
            // 1. Process code
            if (!item.TryGetProperty("code", out var codeProperty))
            {
                return Result.Failure<AttributesValue>(new Error(
                    code: "AttributesValue.Create.IterateArray.TryGetCodeProperty",
                    message: "Array item must constains code property"));
            }

            var codeValue = codeProperty.GetString();
            if (string.IsNullOrWhiteSpace(codeValue))
            {
                return Result.Failure<AttributesValue>(new Error(
                    code: "AttributesValue.Create.IterateArray.CheckCodeValue",
                    message: "Code value can not be null or whitespace"));
            }

            if (map.ContainsKey(codeValue))
            {
                return Result.Failure<AttributesValue>(new Error(
                    code: "AttributesValue.Create.IterateArray.CheckMapContainsCode",
                    message: "The code must not be repeated in the array"));
            }

            // 2. Process attribute
            if (!item.TryGetProperty("attribute", out var attributeValueProperty))
            {
                return Result.Failure<AttributesValue>(new Error(
                    code: "AttributesValue.Create.IterateArray.TryGetAttributeValueProperty",
                    message: "Array item must constains attribute value property"));
            }

            var attributeValue = attributeValueProperty.GetString();
            if (string.IsNullOrWhiteSpace(attributeValue))
            {
                return Result.Failure<AttributesValue>(new Error(
                    code: "AttributesValue.Create.IterateArray.CheckAttributeValue",
                    message: "Attribute value can not be null or whitespace"));
            }

            // 3. Add to map
            map.Add(codeValue, attributeValue);
        }

        return new AttributesValue(map);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        foreach (var item in _values)
        {
            yield return item;
        }
    }
}
