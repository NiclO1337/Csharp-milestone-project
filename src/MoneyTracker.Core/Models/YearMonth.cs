namespace MoneyTracker.Core.Models;

/// <summary>
/// A calendar month a <see cref="Transaction"/> belongs to. Deliberately coarser than
/// <see cref="DateOnly"/>: a transaction has no day, only a month.
/// </summary>
public readonly record struct YearMonth : IComparable<YearMonth>
{
    /// <summary>The earliest year a <see cref="YearMonth"/> may hold.</summary>
    public const int MinYear = 1900;

    /// <summary>The latest year a <see cref="YearMonth"/> may hold.</summary>
    public const int MaxYear = 2100;

    /// <summary>The year component, between <see cref="MinYear"/> and <see cref="MaxYear"/>.</summary>
    public int Year { get; }

    /// <summary>The month component, between 1 and 12.</summary>
    public int Month { get; }

    /// <summary>
    /// Creates a new <see cref="YearMonth"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="year"/> is outside <see cref="MinYear"/>–<see cref="MaxYear"/>, or
    /// <paramref name="month"/> is outside 1–12.
    /// </exception>
    public YearMonth(int year, int month)
    {
        if (year < MinYear || year > MaxYear)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, $"Year must be between {MinYear} and {MaxYear}.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");
        }

        Year = year;
        Month = month;
    }

    /// <summary>
    /// Attempts to parse a <see cref="YearMonth"/> from "yyyy-M" or "yyyy-MM" (e.g. "2026-9" or
    /// "2026-09"). Returns <see langword="false"/> instead of throwing on any invalid input.
    /// </summary>
    public static bool TryParse(string? value, out YearMonth result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split('-');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var year) || !int.TryParse(parts[1], out var month))
        {
            return false;
        }

        if (year < MinYear || year > MaxYear || month is < 1 or > 12)
        {
            return false;
        }

        result = new YearMonth(year, month);
        return true;
    }

    /// <inheritdoc />
    public int CompareTo(YearMonth other)
    {
        var yearComparison = Year.CompareTo(other.Year);
        return yearComparison != 0 ? yearComparison : Month.CompareTo(other.Month);
    }

    /// <summary>Returns the month as "yyyy-MM", e.g. "2026-09".</summary>
    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
