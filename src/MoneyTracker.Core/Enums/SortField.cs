namespace MoneyTracker.Core.Enums;

/// <summary>Which field to sort transactions by.</summary>
public enum SortField
{
    /// <summary>Sort by <c>Transaction.Month</c>.</summary>
    Month,

    /// <summary>Sort by <c>Transaction.SignedAmount</c>.</summary>
    Amount,

    /// <summary>Sort by <c>Transaction.Title</c>.</summary>
    Title,
}
