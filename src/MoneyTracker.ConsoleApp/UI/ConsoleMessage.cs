using System.Globalization;

namespace MoneyTracker.ConsoleApp.UI;

/// <summary>
/// Owns every colour change made to the console. Always calls <see cref="Console.ResetColor"/>
/// afterwards, so a message can never leave later output tinted.
/// </summary>
internal static class ConsoleMessage
{
    internal static void DisplayErrorMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    internal static void DisplaySuccessMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    internal static void DisplayWarningMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    internal static void Heading(string title)
    {
        var culture = CultureInfo.GetCultureInfo("sv-SE");
        var titleCase = culture.TextInfo.ToTitleCase(title.ToLower(culture));
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"\n=== {titleCase} ===\n");
        Console.ResetColor();
    }
}
