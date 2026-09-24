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

    /// <summary>
    /// Writes <paramref name="text"/> in <paramref name="color"/> followed by a newline.
    /// </summary>
    internal static void WriteColoredLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    internal static void Heading(string title, ConsoleColor color = ConsoleColor.DarkCyan)
    {
        Console.Clear();
        var culture = CultureInfo.GetCultureInfo("sv-SE");
        var titleCase = culture.TextInfo.ToTitleCase(title.ToLower(culture));
        WriteColoredLine($"\n=== {titleCase} ===\n", color);
    }

    /// <summary>
    /// Writes the boxed banner shown at the top of the main menu. The border width is derived
    /// from <paramref name="title"/> so it always frames the text with 3 characters to spare on
    /// each side.
    /// </summary>
    internal static void MainHeading(string title, ConsoleColor color = ConsoleColor.DarkYellow)
    {
        Console.Clear();
        var border = new string('=', title.Length + 6);
        WriteColoredLine($"\n\n{border}", color);
        WriteColoredLine($"   {title}", color);
        WriteColoredLine($"{border}\n", color);
    }
}
