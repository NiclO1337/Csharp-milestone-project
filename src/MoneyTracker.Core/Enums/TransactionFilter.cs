namespace MoneyTracker.Core.Enums;

/// <summary>Which transactions to include when listing.</summary>
public enum TransactionFilter
{
    /// <summary>Include both incomes and expenses.</summary>
    All,

    /// <summary>Include only incomes.</summary>
    IncomesOnly,

    /// <summary>Include only expenses.</summary>
    ExpensesOnly,
}
