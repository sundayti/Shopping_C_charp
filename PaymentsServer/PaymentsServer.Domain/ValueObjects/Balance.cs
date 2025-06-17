using System.Globalization;

namespace PaymentsServer.Domain.ValueObjects;

public class Balance
{
    public decimal Value { get; private set; }

    public Balance(decimal value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Balance cannot be negative.");
        Value = value;
    }

    public static implicit operator Balance(decimal value) => new Balance(value);
    public static implicit operator Balance(int value) => new Balance(value);

    public static Balance operator +(Balance a, decimal amount)
    {
        decimal result = a.Value + amount;
        if (result < 0)
            throw new InvalidOperationException("Balance cannot be negative.");
        return new Balance(result);
    }

    public static Balance operator -(Balance a, decimal amount)
    {
        decimal result = a.Value - amount;
        if (result < 0)
            throw new InvalidOperationException("Balance cannot be negative.");
        return new Balance(result);
    }

    public static Balance operator +(Balance a, Balance b) => a + b.Value;
    public static Balance operator -(Balance a, Balance b) => a - b.Value;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}