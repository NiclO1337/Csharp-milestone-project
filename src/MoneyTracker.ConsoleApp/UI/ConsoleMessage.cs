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

    /// <summary>
    /// Writes <paramref name="text"/> in <paramref name="color"/> with <see cref="SlowConsole"/>'s
    /// typewriter effect. Delegates the character-by-character pacing to <see cref="SlowConsole"/>
    /// so this stays the only place that sets and resets colour.
    /// </summary>
    internal static void WriteColoredSlow(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        SlowConsole.TypeTextSlow(text);
        Console.ResetColor();
    }

    /// <summary>
    /// Writes a blank line. Exists so callers outside <c>UI/</c> never need to call
    /// <see cref="Console.WriteLine()"/> directly.
    /// </summary>
    internal static void NewLine() => Console.WriteLine();

    internal static void Heading(string title, ConsoleColor color = ConsoleColor.DarkCyan)
    {
        ClearScreen();
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
        ClearScreen();
        var border = new string('=', title.Length + 6);
        WriteColoredLine($"\n\n{border}", color);
        WriteColoredLine($"   {title}", color);
        WriteColoredLine($"{border}\n", color);
    }

    /// <summary>
    /// Clears the visible screen and the terminal's scrollback. <see cref="Console.Clear"/>
    /// alone only wipes the current viewport — terminals such as Windows Terminal keep prior
    /// screens in a separate scrollback buffer, which is what's left over when the user scrolls
    /// up after navigating menus. The VT sequence clears that history too.
    /// </summary>
    private static void ClearScreen()
    {
        Console.Clear();
        Console.Write("\x1b[3J");
    }
}
