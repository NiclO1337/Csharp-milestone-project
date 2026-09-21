using System.Text.Json.Serialization;

namespace MoneyTracker.Core.Models;

/// <summary>A transaction that adds to the balance.</summary>
public sealed class Income : Transaction
{
    /// <exception cref="ArgumentException"><paramref name="title"/> is blank or too long.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="amount"/> is not greater than 0 or exceeds <see cref="Transaction.MaxAmount"/>.
    /// </exception>
    [JsonConstructor]
    public Income(int id, string title, decimal amount, YearMonth month)
        : base(id, title, amount, month)
    {
    }

    /// <inheritdoc />
    public override decimal SignedAmount => Amount;
}
