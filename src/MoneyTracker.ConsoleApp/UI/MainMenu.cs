using System.Globalization;
using MoneyTracker.Core.Exceptions;
using MoneyTracker.Core.Models;
using MoneyTracker.Core.Services;

namespace MoneyTracker.ConsoleApp.UI;

/// <summary>
/// The console UI's root shell: the top-level menu loop and its "Quit" condition. Each of the
/// six actions is delegated to <see cref="TransactionMenu"/> — this class owns navigation only.
/// </summary>
internal sealed class MainMenu
{
    private static readonly CultureInfo s_currency = CultureInfo.GetCultureInfo("sv-SE");

    private readonly TransactionService _service;
    private readonly TransactionMenu _transactionMenu;

    internal MainMenu(TransactionService service)
    {
        _service = service;
        _transactionMenu = new TransactionMenu(service);
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
        string[] menuItems =
        [
            "Show transactions",
            "Add income",
            "Add expense",
            "Edit transaction",
            "Remove transaction",
            "Monthly summary",
        ];

        while (true)
        {
            Console.WriteLine("\n\n=============================================");
            Console.WriteLine("   DRAGON'S LEDGER - MONEY TRACKING SYSTEM");
            Console.WriteLine("=============================================\n");
            PrintSummary(_service.GetSummary());
            Console.WriteLine();

            var choice = ConsoleInput.SelectMenuOption(menuItems, "Quit");

            switch (choice)
            {
                case 1:
                    // Only case that skips the pause: returning from "Back to main menu" should
                    // re-render instantly, not make browsing feel sluggish.
                    _transactionMenu.ShowTransactions();
                    break;
                case 2:
                    ConsoleInput.TryRun(_transactionMenu.AddIncome);
                    ConsoleInput.Pause();
                    break;
                case 3:
                    ConsoleInput.TryRun(_transactionMenu.AddExpense);
                    ConsoleInput.Pause();
                    break;
                case 4:
                    ConsoleInput.TryRun(_transactionMenu.EditTransaction);
                    ConsoleInput.Pause();
                    break;
                case 5:
                    ConsoleInput.TryRun(_transactionMenu.RemoveTransaction);
                    ConsoleInput.Pause();
                    break;
                case 6:
                    ConsoleInput.TryRun(_transactionMenu.ShowMonthlySummary);
                    ConsoleInput.Pause();
                    break;
                case 0:
                    return;
            }
        }
    }

    private static void PrintSummary(BalanceSummary summary)
    {
        Console.WriteLine(
            $"Balance: {summary.Balance.ToString("C0", s_currency)}   " +
            $"(income {summary.TotalIncome.ToString("C0", s_currency)} · expenses {summary.TotalExpenses.ToString("C0", s_currency)})");
    }
}
