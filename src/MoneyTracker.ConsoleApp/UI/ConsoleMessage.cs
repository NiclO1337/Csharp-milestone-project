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
        Console.ForegroundColor = ConsoleColor.DarkRed;
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

    /// <summary>
    /// Writes <paramref name="text"/> in <paramref name="color"/> without a trailing newline, so
    /// it can be combined with plain-colour text on the same line (e.g. one coloured column in
    /// an otherwise uncoloured table row).
    /// </summary>
    internal static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
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
