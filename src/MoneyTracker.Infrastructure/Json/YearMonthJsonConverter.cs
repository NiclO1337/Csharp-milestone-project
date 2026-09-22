using System.Text.Json;
using System.Text.Json.Serialization;
using MoneyTracker.Core.Models;

namespace MoneyTracker.Infrastructure.Json;

/// <summary>
/// Serializes <see cref="YearMonth"/> as "yyyy-MM" (e.g. "2026-09") instead of a nested object.
/// Registered on <see cref="JsonSerializerOptions"/> by <see cref="JsonTransactionRepository"/>
/// rather than declared as an attribute on <see cref="YearMonth"/> — serialization format is an
/// Infrastructure concern, not a domain one.
/// </summary>
internal sealed class YearMonthJsonConverter : JsonConverter<YearMonth>
{
    /// <inheritdoc />
    public override YearMonth Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (!YearMonth.TryParse(value, out var yearMonth))
        {
            throw new JsonException($"'{value}' is not a valid month (expected \"yyyy-MM\").");
        }

        return yearMonth;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, YearMonth value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
