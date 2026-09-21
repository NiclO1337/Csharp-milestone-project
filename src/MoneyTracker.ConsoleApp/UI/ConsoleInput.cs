namespace MoneyTracker.ConsoleApp.UI;

/// <summary>
/// Reads and validates console input in a re-prompt loop. Never returns invalid data — bad
/// input is a normal event handled here, not an exception thrown at the caller.
/// </summary>
internal static class ConsoleInput
{
    internal static string ValidateInput(string prompt, int maxLength, bool allowCancel = false, string? currentValue = null)
    {
        while (true)
        {
            Console.Write("\n" + prompt);
            var input = Console.ReadLine();

            if (allowCancel && IsCancel(input))
            {
                throw new UserCancelledException();
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                if (currentValue is not null)
                {
                    return currentValue;
                }

                ConsoleMessage.DisplayErrorMessage("Input can not be empty.");
                continue;
            }

            if (input.Length > maxLength)
            {
                ConsoleMessage.DisplayErrorMessage($"Input can be maximum {maxLength} characters.");
                continue;
            }

            return input;
        }
    }

    internal static T ValidateInput<T>(
        string prompt,
        Func<string, (bool isValid, T result)> validator,
        string errorMessage = "Invalid input, try again.",
        bool allowCancel = false,
        bool hasCurrentValue = false,
        T currentValue = default!)
    {
        while (true)
        {
            Console.Write("\n" + prompt);
            var input = Console.ReadLine();

            if (allowCancel && IsCancel(input))
            {
                throw new UserCancelledException();
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                if (hasCurrentValue)
                {
                    return currentValue;
                }

                ConsoleMessage.DisplayErrorMessage("Input can not be empty.");
                continue;
            }

            var (isValid, result) = validator(input);
            if (isValid)
            {
                return result;
            }

            ConsoleMessage.DisplayErrorMessage(errorMessage);
        }
    }

    internal static Func<string, (bool isValid, int result)> ValidateIntegerRange(int min, int max)
    {
        return input =>
        {
            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return (true, value);
            }

            return (false, 0);
        };
    }

    internal static void DisplayNumberedList<T>(IReadOnlyList<T> items, Func<T, string> display)
    {
        for (var i = 0; i < items.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {display(items[i])}");
        }
    }

    internal static void DisplayNumberedList(IReadOnlyList<string> items) => DisplayNumberedList(items, item => item);

    internal static T SelectFromList<T>(
        IReadOnlyList<T> items,
        Func<T, string> display,
        string prompt = "Select an option: ",
        bool allowCancel = false)
    {
        DisplayNumberedList(items, display);

        var choice = ValidateInput(
            prompt,
            ValidateIntegerRange(1, items.Count),
            $"Invalid input, please enter a number between 1 and {items.Count}.",
            allowCancel);

        return items[choice - 1];
    }

    internal static bool Confirm(string message)
    {
        ConsoleMessage.DisplayWarningMessage(message);
        Console.WriteLine("1. Yes");
        Console.WriteLine("2. No");

        var choice = ValidateInput("Select option (1 - 2): ", ValidateIntegerRange(1, 2), "Invalid input, select 1 or 2.");
        return choice == 1;
    }

    /// <summary>
    /// Runs a menu action and silently handles user-initiated cancellation
    /// (<see cref="UserCancelledException"/>), returning to the calling menu instead of crashing.
    /// </summary>
    internal static void TryRun(Action action)
    {
        try
        {
            action();
        }
        catch (UserCancelledException)
        {
            ConsoleMessage.DisplayWarningMessage("Cancelled - returning to previous menu.");
        }
    }

    private static bool IsCancel(string? input) => input is not null && input.Trim().Equals("q", StringComparison.OrdinalIgnoreCase);
}
