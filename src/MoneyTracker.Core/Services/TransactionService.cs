using MoneyTracker.Core.Abstractions;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.Exceptions;
using MoneyTracker.Core.Models;

namespace MoneyTracker.Core.Services;

/// <summary>
/// The single entry point to all transaction business logic. Owns the in-memory list, assigns
/// IDs, and persists after every mutation.
/// </summary>
public sealed class TransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly List<Transaction> _transactions;
    private int _nextId;

    /// <summary>
    /// Loads every transaction from <paramref name="repository"/> and assigns the next ID from
    /// the highest existing one, so IDs stay unique and unreused across restarts.
    /// </summary>
    /// <exception cref="DataStoreException">The store exists but could not be read.</exception>
    public TransactionService(ITransactionRepository repository)
    {
        _repository = repository;
        _transactions = repository.Load().ToList();
        _nextId = _transactions.Count == 0 ? 1 : _transactions.Max(t => t.Id) + 1;
    }

    /// <summary>Returns transactions matching <paramref name="filter"/>, sorted as requested.</summary>
    public IReadOnlyList<Transaction> GetTransactions(
        TransactionFilter filter = TransactionFilter.All,
        SortField sortBy = SortField.Month,
        SortDirection direction = SortDirection.Descending)
    {
        var filtered = FilterBy(filter);

        IOrderedEnumerable<Transaction> sorted = (sortBy, direction) switch
        {
            (SortField.Amount, SortDirection.Ascending) => filtered.OrderBy(t => t.SignedAmount),
            (SortField.Amount, SortDirection.Descending) => filtered.OrderByDescending(t => t.SignedAmount),
            (SortField.Title, SortDirection.Ascending) => filtered.OrderBy(t => t.Title, StringComparer.CurrentCultureIgnoreCase),
            (SortField.Title, SortDirection.Descending) => filtered.OrderByDescending(t => t.Title, StringComparer.CurrentCultureIgnoreCase),
            // ThenByDescending on both branches is deliberate: incomes (positive SignedAmount)
            // should sort before expenses (negative) within the same month regardless of which
            // way Month itself is ordered.
            (_, SortDirection.Ascending) => filtered.OrderBy(t => t.Month).ThenByDescending(t => t.SignedAmount),
            (_, SortDirection.Descending) => filtered.OrderByDescending(t => t.Month).ThenByDescending(t => t.SignedAmount),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown sort direction."),
        };

        return sorted.ToList();
    }

    /// <summary>
    /// Sums <c>SignedAmount</c> across transactions matching <paramref name="filter"/> — the
    /// incomes total, the expenses total (negative), or the net balance when both are included.
    /// </summary>
    public decimal GetTotal(TransactionFilter filter = TransactionFilter.All) =>
        FilterBy(filter).Sum(t => t.SignedAmount);

    private IEnumerable<Transaction> FilterBy(TransactionFilter filter) => filter switch
    {
        TransactionFilter.IncomesOnly => _transactions.Where(t => t.SignedAmount > 0),
        TransactionFilter.ExpensesOnly => _transactions.Where(t => t.SignedAmount < 0),
        _ => _transactions,
    };

    /// <summary>Finds a transaction by ID, or <see langword="null"/> if none exists.</summary>
    public Transaction? FindById(int id) => _transactions.FirstOrDefault(t => t.Id == id);

    /// <summary>
    /// Returns transactions for a single <paramref name="month"/>, or every transaction if
    /// <paramref name="month"/> is <see langword="null"/> — sorted the same way as
    /// <see cref="GetTransactions"/>'s default (most recent month first).
    /// </summary>
    public IReadOnlyList<Transaction> GetTransactionsForMonth(YearMonth? month = null)
    {
        var transactions = GetTransactions(sortBy: SortField.Month, direction: SortDirection.Descending);
        return month is null ? transactions : transactions.Where(t => t.Month == month.Value).ToList();
    }

    /// <summary>Distinct months that have at least one transaction, oldest first.</summary>
    public IReadOnlyList<YearMonth> GetAvailableMonths() =>
        _transactions.Select(t => t.Month).Distinct().OrderBy(m => m).ToList();

    /// <summary>Adds a new income and persists.</summary>
    /// <exception cref="ArgumentException"><paramref name="title"/> is blank or too long.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is out of range.</exception>
    public Income AddIncome(string title, decimal amount, YearMonth month)
    {
        var income = new Income(_nextId, title, amount, month);
        _nextId++;
        _transactions.Add(income);
        Persist();
        return income;
    }

    /// <summary>Adds a new expense and persists.</summary>
    /// <exception cref="ArgumentException"><paramref name="title"/> is blank or too long.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is out of range.</exception>
    public Expense AddExpense(string title, decimal amount, YearMonth month)
    {
        var expense = new Expense(_nextId, title, amount, month);
        _nextId++;
        _transactions.Add(expense);
        Persist();
        return expense;
    }

    /// <summary>Updates an existing transaction in place and persists.</summary>
    /// <exception cref="TransactionNotFoundException">No transaction has <paramref name="id"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="title"/> is blank or too long.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is out of range.</exception>
    public Transaction Update(int id, string title, decimal amount, YearMonth month)
    {
        var transaction = FindById(id) ?? throw new TransactionNotFoundException(id);
        transaction.Update(title, amount, month);
        Persist();
        return transaction;
    }

    /// <summary>Removes a transaction and persists.</summary>
    /// <exception cref="TransactionNotFoundException">No transaction has <paramref name="id"/>.</exception>
    public void Remove(int id)
    {
        var transaction = FindById(id) ?? throw new TransactionNotFoundException(id);
        _transactions.Remove(transaction);
        Persist();
    }

    /// <summary>
    /// Totals income, expenses and balance, either across all transactions or restricted to a
    /// single <paramref name="month"/>.
    /// </summary>
    public BalanceSummary GetSummary(YearMonth? month = null)
    {
        IEnumerable<Transaction> relevant = month is null
            ? _transactions
            : _transactions.Where(t => t.Month == month.Value);

        var totalIncome = relevant.Where(t => t.SignedAmount > 0).Sum(t => t.SignedAmount);
        var totalExpenses = relevant.Where(t => t.SignedAmount < 0).Sum(t => -t.SignedAmount);

        return new BalanceSummary(totalIncome, totalExpenses);
    }

    private void Persist() => _repository.Save(_transactions);
}
