using System.Text.Json.Serialization;

namespace MoneyTracker.Core.Models;

/// <summary>
/// A single income or expense entry. Concrete meaning comes from the subclass: <see cref="Amount"/>
/// is always positive, and <see cref="SignedAmount"/> is the polymorphic hook that applies the sign.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Income), "income")]
[JsonDerivedType(typeof(Expense), "expense")]
public abstract class Transaction
{
    /// <summary>The maximum allowed length of <see cref="Title"/>.</summary>
    public const int MaxTitleLength = 60;

    /// <summary>The maximum allowed value of <see cref="Amount"/>.</summary>
    public const decimal MaxAmount = 1_000_000_000m;

    /// <summary>Unique, stable identifier assigned by <c>TransactionService</c>. Never reused.</summary>
    public int Id { get; }

    /// <summary>Trimmed, non-empty title, at most <see cref="MaxTitleLength"/> characters.</summary>
    public string Title { get; private set; }

    /// <summary>Always positive, at most <see cref="MaxAmount"/>, rounded to 2 decimals.</summary>
    public decimal Amount { get; private set; }

    /// <summary>The calendar month this transaction belongs to.</summary>
    public YearMonth Month { get; private set; }

    /// <summary>
    /// <see cref="Amount"/> with the sign this transaction type contributes to a balance:
    /// positive for income, negative for an expense. Derived, not stored.
    /// </summary>
    [JsonIgnore]
    public abstract decimal SignedAmount { get; }

    /// <summary>Display label for this transaction's kind, e.g. "Income" or "Expense". Derived, not stored.</summary>
    [JsonIgnore]
    public abstract string TypeName { get; }

    /// <exception cref="ArgumentException"><paramref name="title"/> is blank or too long.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="amount"/> is not greater than 0 or exceeds <see cref="MaxAmount"/>.
    /// </exception>
    protected Transaction(int id, string title, decimal amount, YearMonth month)
    {
        Id = id;
        Title = ValidateTitle(title);
        Amount = ValidateAmount(amount);
        Month = month;
    }

    /// <summary>
    /// Replaces <see cref="Title"/>, <see cref="Amount"/> and <see cref="Month"/> in place,
    /// re-validating exactly as the constructor does. <see cref="Id"/> never changes.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="title"/> is blank or too long.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="amount"/> is not greater than 0 or exceeds <see cref="MaxAmount"/>.
    /// </exception>
    public void Update(string title, decimal amount, YearMonth month)
    {
        Title = ValidateTitle(title);
        Amount = ValidateAmount(amount);
        Month = month;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be blank.", nameof(title));
        }

        var trimmed = title.Trim();
        if (trimmed.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters.", nameof(title));
        }

        return trimmed;
    }

    private static decimal ValidateAmount(decimal amount)
    {
        if (amount <= 0 || amount > MaxAmount)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, $"Amount must be greater than 0 and at most {MaxAmount}.");
        }

        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
