using System.Globalization;
using MoneyTracker.Core.Models;

namespace MoneyTracker.ConsoleApp.UI;

/// <summary>
/// Renders a list of transactions as a table whose column widths fit the actual data.
/// Presentation only — no sorting, filtering, or arithmetic; callers pass in exactly what
/// should be shown.
/// </summary>
internal static class TransactionTable
{
    private const int ColumnPadding = 3;

    private static readonly CultureInfo s_currency = CultureInfo.GetCultureInfo("sv-SE");

    /// <summary>
    /// Renders the table and returns its usable width — <c>header.Length - ColumnPadding</c>,
    /// the same span the border rules cover — so callers can align extra content (e.g. a total)
    /// to the table's edge. Returns <see langword="null"/> if there was nothing to render.
    /// </summary>
    internal static int? Display(IReadOnlyList<Transaction> transactions)
    {
        if (transactions.Count == 0)
        {
            ConsoleMessage.DisplayWarningMessage("No transactions to show.");
            return null;
        }

        var rows = transactions.Select(ToRow).ToList();
        var widths = MeasureColumns(rows);
        var header = FormatRow(new Row("ID", "Type", "Title", "Month", "Year", "Amount (SEK)"), widths);
        
        var tableWidth = header.Length - ColumnPadding;

        Console.WriteLine(header);
        Console.WriteLine(new string('-', tableWidth));

        foreach (var row in rows)
        {
            Console.WriteLine(FormatRow(row, widths));
        }

        Console.WriteLine(new string('-', tableWidth));

        return tableWidth;
    }

    private static Row ToRow(Transaction transaction)
    {
        var date = new DateOnly(transaction.Month.Year, transaction.Month.Month, 1);

        return new Row(
            transaction.Id.ToString(CultureInfo.InvariantCulture),
            transaction.TypeName,
            transaction.Title,
            date.ToString("MMMM", CultureInfo.InvariantCulture),
            date.ToString("yyyy", CultureInfo.InvariantCulture),
            transaction.SignedAmount.ToString("N0", s_currency));
    }

    private static ColumnWidths MeasureColumns(IReadOnlyList<Row> rows) => new(
        Id: Math.Max("ID".Length, rows.Max(r => r.Id.Length)) + ColumnPadding,
        Type: Math.Max("Type".Length, rows.Max(r => r.Type.Length)) + ColumnPadding,
        Title: Math.Max("Title".Length, rows.Max(r => r.Title.Length)) + ColumnPadding,
        Month: Math.Max("Month".Length, rows.Max(r => r.Month.Length)) + ColumnPadding,
        Year: Math.Max("Year".Length, rows.Max(r => r.Year.Length)) + ColumnPadding,
        Amount: Math.Max("Amount (SEK)".Length, rows.Max(r => r.Amount.Length)) + ColumnPadding);

    private static string FormatRow(Row row, ColumnWidths widths) =>
        PadNumeric(row.Id, widths.Id)
        + row.Type.PadRight(widths.Type)
        + row.Title.PadRight(widths.Title)
        + row.Month.PadRight(widths.Month)
        + row.Year.PadRight(widths.Year)
        + PadNumeric(row.Amount, widths.Amount);

    /// <summary>
    /// Right-aligns <paramref name="value"/>, then appends the column gap as trailing spaces.
    /// Plain <see cref="string.PadLeft(int)"/> puts all padding on the left, leaving no gap
    /// before the next column — unlike <see cref="string.PadRight(int)"/> columns, whose
    /// padding already trails.
    /// </summary>
    private static string PadNumeric(string value, int totalWidth) =>
        value.PadLeft(totalWidth - ColumnPadding) + new string(' ', ColumnPadding);

    private readonly record struct Row(string Id, string Type, string Title, string Month, string Year, string Amount);

    private readonly record struct ColumnWidths(int Id, int Type, int Title, int Month, int Year, int Amount);
}
