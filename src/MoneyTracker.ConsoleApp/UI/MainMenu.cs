using System.Globalization;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.Exceptions;
using MoneyTracker.Core.Models;
using MoneyTracker.Core.Services;

namespace MoneyTracker.ConsoleApp.UI;

/// <summary>
/// The console UI's main loop. Reads intent via <see cref="ConsoleInput"/>, asks
/// <see cref="TransactionService"/> to do the work, and renders the result — no sorting,
/// filtering, or arithmetic happens here.
/// </summary>
internal sealed class MainMenu
{
    private static readonly CultureInfo s_currency = CultureInfo.GetCultureInfo("sv-SE");

    private readonly TransactionService _service;

    internal MainMenu(TransactionService service)
    {
        _service = service;
    }

    internal void Run()
    {
        try
        {
            RunMenuLoop();
        }
        catch (UserCancelledException)
        {
            // Input stream closed (e.g. piped input reached its end) while reading the top-level
            // menu choice, which isn't wrapped by ConsoleInput.TryRun. Exit gracefully, same as
            // choosing "0. Quit".
        }
    }

    private void RunMenuLoop()
    {
        while (true)
        {
            ConsoleMessage.Heading("MoneyTracker");
            PrintSummary(_service.GetSummary());

            Console.WriteLine();
            Console.WriteLine("1. Show transactions");
            Console.WriteLine("2. Add income");
            Console.WriteLine("3. Add expense");
            Console.WriteLine("4. Edit transaction");
            Console.WriteLine("5. Remove transaction");
            Console.WriteLine("6. Monthly summary");
            Console.WriteLine("0. Quit");

            var choice = ConsoleInput.ValidateInput(
                "Select option (0 - 6): ",
                ConsoleInput.ValidateIntegerRange(0, 6),
                "Invalid input, select a number between 0 and 6.");

            switch (choice)
            {
                case 1:
                    ConsoleInput.TryRun(ShowTransactions);
                    break;
                case 2:
                    ConsoleInput.TryRun(AddIncome);
                    break;
                case 3:
                    ConsoleInput.TryRun(AddExpense);
                    break;
                case 4:
                    ConsoleInput.TryRun(EditTransaction);
                    break;
                case 5:
                    ConsoleInput.TryRun(RemoveTransaction);
                    break;
                case 6:
                    ConsoleInput.TryRun(ShowMonthlySummary);
                    break;
                case 0:
                    return;
            }
        }
    }

    private void ShowTransactions()
    {
        ConsoleMessage.Heading("Show Transactions");

        var filter = ConsoleInput.SelectFromList(Enum.GetValues<TransactionFilter>(), f => f.ToString(), "Filter: ", allowCancel: true);
        var sortBy = ConsoleInput.SelectFromList(Enum.GetValues<SortField>(), f => f.ToString(), "Sort by: ", allowCancel: true);
        var direction = ConsoleInput.SelectFromList(Enum.GetValues<SortDirection>(), d => d.ToString(), "Direction: ", allowCancel: true);

        TransactionTable.Display(_service.GetTransactions(filter, sortBy, direction));
    }

    private void AddIncome() =>
        AddTransaction("Add Income", (title, amount, month) => _service.AddIncome(title, amount, month));

    private void AddExpense() =>
        AddTransaction("Add Expense", (title, amount, month) => _service.AddExpense(title, amount, month));

    private void AddTransaction(string heading, Func<string, decimal, YearMonth, Transaction> add)
    {
        ConsoleMessage.Heading(heading);

        var title = ConsoleInput.ValidateInput("Title (q to cancel): ", Transaction.MaxTitleLength, allowCancel: true);
        var amount = ConsoleInput.ValidateInput(
            "Amount (q to cancel): ",
            ValidateAmount,
            "Invalid amount, must be greater than 0.",
            allowCancel: true);
        var month = ConsoleInput.ValidateInput(
            "Month yyyy-MM (q to cancel): ",
            ValidateMonth,
            "Invalid month, expected format yyyy-MM.",
            allowCancel: true);

        var transaction = add(title, amount, month);
        ConsoleMessage.DisplaySuccessMessage($"{transaction.TypeName} '{transaction.Title}' added.");
    }

    private void EditTransaction()
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

    private void RemoveTransaction()
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

    private void ShowMonthlySummary()
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
