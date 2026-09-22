using System.Globalization;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.Exceptions;
using MoneyTracker.Core.Models;
using MoneyTracker.Core.Services;

namespace MoneyTracker.ConsoleApp.UI;

/// <summary>
/// The six transaction-facing screens reachable from <see cref="MainMenu"/>: show (with
/// pagination and filter/sort/direction), add, edit, remove, and monthly summary.
/// </summary>
internal sealed class TransactionMenu
{
    private const int PageSize = 10;

    private static readonly CultureInfo s_currency = CultureInfo.GetCultureInfo("sv-SE");

    private readonly TransactionService _service;

    internal TransactionMenu(TransactionService service)
    {
        _service = service;
    }

    /// <summary>
    /// Entirely menu-driven — every step is a numbered choice with a reserved "0. Back", so
    /// there's never a raw free-text prompt a user could get stuck in. That's why this isn't
    /// wrapped in <see cref="ConsoleInput.TryRun"/>: there's nothing here <c>allowCancel</c>
    /// would ever need to escape.
    /// </summary>
    internal void ShowTransactions()
    {
        var filter = TransactionFilter.All;
        var sortBy = SortField.Month;
        var direction = SortDirection.Descending;
        var page = 0;

        while (true)
        {
            var transactions = _service.GetTransactions(filter, sortBy, direction);
            var pageCount = Math.Max(1, (transactions.Count + PageSize - 1) / PageSize);

            ConsoleMessage.Heading("Show Transactions");
            TransactionTable.Display(transactions.Skip(page * PageSize).Take(PageSize).ToList());
            Console.WriteLine($"\nPage {page + 1} of {pageCount}");
            Console.WriteLine();

            string[] menuItems =
            [
                "Previous page",
                "Next page\n",
                $"Change filter (current: {filter})",
                $"Change sort (current: {sortBy})",
                $"Toggle direction (current: {direction})",
            ];

            var choice = ConsoleInput.SelectMenuOption(menuItems, "Back to main menu");

            switch (choice)
            {
                case 1:
                    if (page == 0)
                    {
                        ConsoleMessage.DisplayWarningMessage("Already on the first page.");
                    }
                    else
                    {
                        page--;
                    }

                    break;
                case 2:
                    if (page == pageCount - 1)
                    {
                        ConsoleMessage.DisplayWarningMessage("Already on the last page.");
                    }
                    else
                    {
                        page++;
                    }

                    break;
                case 3:
                    (filter, page) = ApplyChange(filter, page);
                    break;
                case 4:
                    (sortBy, page) = ApplyChange(sortBy, page);
                    break;
                case 5:
                    direction = direction == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
                    page = 0;
                    break;
                case 0:
                    return;
            }
        }
    }

    /// <summary>Applies a new value for one of <see cref="ShowTransactions"/>'s settings, resetting to page 1 only if it actually changed.</summary>
    private static (T Value, int Page) ApplyChange<T>(T current, int page)
        where T : struct, Enum
    {
        var updated = ConsoleInput.SelectEnumOption(current);
        return updated.Equals(current) ? (current, page) : (updated, 0);
    }

    private static readonly string[] s_incomeTitles = ["Salary", "Pension", "Freelance", "Gift", "Bonus"];

    private static readonly string[] s_expenseTitles =
    [
        "Rent", "Electricity", "Water", "Internet", "Phone", "Groceries", "Clothing",
        "Transport", "Insurance", "Subscription", "Dining Out", "Entertainment",
        "Healthcare", "Gym", "Travel",
    ];

    internal void AddIncome() =>
        AddTransaction("Add Income", s_incomeTitles, (title, amount, month) => _service.AddIncome(title, amount, month));

    internal void AddExpense() =>
        AddTransaction("Add Expense", s_expenseTitles, (title, amount, month) => _service.AddExpense(title, amount, month));

    private static void AddTransaction(string heading, IReadOnlyList<string> titlePresets, Func<string, decimal, YearMonth, Transaction> add)
    {
        ConsoleMessage.Heading(heading);

        var title = SelectTitle(titlePresets);
        var amount = ConsoleInput.ValidateInput(
            "Amount (q to cancel): ",
            ValidateAmount,
            "Invalid amount, must be greater than 0.",
            allowCancel: true);
        var month = ConsoleInput.ValidateInput(
            "Month yyyy-MM (q to cancel or just press Enter for current month): ",
            ValidateMonth,
            "Invalid month, expected format yyyy-MM.",
            allowCancel: true,
            hasCurrentValue: true,
            currentValue: YearMonth.Current);

        var transaction = add(title, amount, month);
        ConsoleMessage.DisplaySuccessMessage($"{transaction.TypeName} '{transaction.Title}' added.");
    }

    private static string SelectTitle(IReadOnlyList<string> presets)
    {
        var menuItems = presets.Append("Write your own title").ToArray();
        var choice = ConsoleInput.SelectMenuOption(menuItems, "Cancel");

        if (choice == 0)
        {
            throw new UserCancelledException();
        }

        return choice == menuItems.Length
            ? ConsoleInput.ValidateInput("Title (q to cancel): ", Transaction.MaxTitleLength, allowCancel: true)
            : presets[choice - 1];
    }

    internal void EditTransaction()
    {
        ConsoleMessage.Heading("Edit Transaction");

        var id = ConsoleInput.ValidateInput(
            "Transaction ID (q to cancel): ",
            ConsoleInput.ValidateIntegerRange(1, int.MaxValue),
            "Invalid input, enter a positive number.",
            allowCancel: true);

        var transaction = _service.FindById(id);
        if (transaction is null)
        {
            ConsoleMessage.DisplayErrorMessage($"No transaction found with ID {id}.");
            return;
        }

        Console.WriteLine($"\nCurrent title:  {transaction.Title}");
        Console.WriteLine($"Current amount: {transaction.Amount.ToString("N2", s_currency)}");
        Console.WriteLine($"Current month:  {transaction.Month}");

        var title = ConsoleInput.ValidateInput(
            "New title (Enter to keep current, q to cancel): ",
            Transaction.MaxTitleLength,
            allowCancel: true,
            currentValue: transaction.Title);
        var amount = ConsoleInput.ValidateInput(
            "New amount (Enter to keep current, q to cancel): ",
            ValidateAmount,
            "Invalid amount, must be greater than 0.",
            allowCancel: true,
            hasCurrentValue: true,
            currentValue: transaction.Amount);
        var month = ConsoleInput.ValidateInput(
            "New month yyyy-MM (Enter to keep current, q to cancel): ",
            ValidateMonth,
            "Invalid month, expected format yyyy-MM.",
            allowCancel: true,
            hasCurrentValue: true,
            currentValue: transaction.Month);

        try
        {
            _service.Update(id, title, amount, month);
            ConsoleMessage.DisplaySuccessMessage("Transaction updated.");
        }
        catch (TransactionNotFoundException)
        {
            ConsoleMessage.DisplayErrorMessage($"No transaction found with ID {id}.");
        }
    }

    internal void RemoveTransaction()
    {
        ConsoleMessage.Heading("Remove Transaction");

        var id = ConsoleInput.ValidateInput(
            "Transaction ID (q to cancel): ",
            ConsoleInput.ValidateIntegerRange(1, int.MaxValue),
            "Invalid input, enter a positive number.",
            allowCancel: true);

        var transaction = _service.FindById(id);
        if (transaction is null)
        {
            ConsoleMessage.DisplayErrorMessage($"No transaction found with ID {id}.");
            return;
        }

        TransactionTable.Display([transaction]);

        if (!ConsoleInput.Confirm($"Remove this {transaction.TypeName.ToLowerInvariant()}?"))
        {
            ConsoleMessage.DisplayWarningMessage("Cancelled.");
            return;
        }

        try
        {
            _service.Remove(id);
            ConsoleMessage.DisplaySuccessMessage("Transaction removed.");
        }
        catch (TransactionNotFoundException)
        {
            ConsoleMessage.DisplayErrorMessage($"No transaction found with ID {id}.");
        }
    }

    internal void ShowMonthlySummary()
    {
        ConsoleMessage.Heading("Monthly Summary");

        var month = ConsoleInput.ValidateInput<YearMonth?>(
            "Month yyyy-MM (Enter for all time, q to cancel): ",
            ValidateOptionalMonth,
            "Invalid month, expected format yyyy-MM.",
            allowCancel: true,
            hasCurrentValue: true,
            currentValue: null);

        Console.WriteLine();
        Console.WriteLine(month is null ? "All time:" : $"{month}:");
        PrintSummary(_service.GetSummary(month));
    }

    private static void PrintSummary(BalanceSummary summary)
    {
        Console.WriteLine(
            $"Balance: {summary.Balance.ToString("C", s_currency)}   " +
            $"(income {summary.TotalIncome.ToString("C", s_currency)} · expenses {summary.TotalExpenses.ToString("C", s_currency)})");
    }

    private static (bool isValid, decimal result) ValidateAmount(string input)
    {
        var normalized = input.Replace(',', '.');
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            && amount > 0
            && amount <= Transaction.MaxAmount)
        {
            return (true, amount);
        }

        return (false, 0m);
    }

    private static (bool isValid, YearMonth result) ValidateMonth(string input) =>
        YearMonth.TryParse(input, out var month) ? (true, month) : (false, default);

    private static (bool isValid, YearMonth? result) ValidateOptionalMonth(string input) =>
        YearMonth.TryParse(input, out var month) ? (true, month) : (false, null);
}
