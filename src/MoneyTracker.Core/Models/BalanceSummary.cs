namespace MoneyTracker.Core.Models;

/// <summary>Aggregated totals over a set of transactions, so the UI never does arithmetic itself.</summary>
public readonly record struct BalanceSummary
{
    /// <summary>Sum of all income amounts.</summary>
    public decimal TotalIncome { get; }

    /// <summary>Sum of all expense amounts, as a positive figure.</summary>
    public decimal TotalExpenses { get; }

    /// <summary><see cref="TotalIncome"/> minus <see cref="TotalExpenses"/>.</summary>
    public decimal Balance { get; }

    /// <summary>Creates a new <see cref="BalanceSummary"/> from the two totals.</summary>
    public BalanceSummary(decimal totalIncome, decimal totalExpenses)
    {
        TotalIncome = totalIncome;
        TotalExpenses = totalExpenses;
        Balance = totalIncome - totalExpenses;
    }
}
