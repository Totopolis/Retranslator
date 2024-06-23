using Domain.Primitives;

namespace Domain.Entities.Payment;

public sealed class SumValue : ValueObject
{
    public SumValue(double amount, int currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public double Amount { get; set; }

    public int Currency { get; set; }

    public static SumValue Create(double amount, int currency)
    {
        if (amount < 0)
        {
            // TODO: use domain errors
            throw new ArgumentException("Bad amount value");
        }

        if (currency != 810)
        {
            // TODO: use domain errors & avoid magic currency number
            throw new ArgumentException("Unknown currency number");
        }

        return new SumValue(amount, currency);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }
}
