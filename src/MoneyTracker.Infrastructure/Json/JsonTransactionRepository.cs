using System.Text.Json;
using MoneyTracker.Core.Abstractions;
using MoneyTracker.Core.Exceptions;
using MoneyTracker.Core.Models;

namespace MoneyTracker.Infrastructure.Json;

/// <summary>
/// Stores transactions as a single JSON file at a fixed path, written atomically so a crash
/// mid-save cannot corrupt the file.
/// </summary>
public sealed class JsonTransactionRepository : ITransactionRepository
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options;

    /// <summary>Creates a repository backed by the JSON file at <paramref name="path"/>.</summary>
    public JsonTransactionRepository(string path)
    {
        _path = path;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new YearMonthJsonConverter() },
        };
    }

    /// <inheritdoc />
    /// <exception cref="DataStoreException">The file exists but could not be read or parsed.</exception>
    public IReadOnlyList<Transaction> Load()
    {
        if (!File.Exists(_path))
        {
            return [];
        }

        string json;
        try
        {
            json = File.ReadAllText(_path);
        }
        catch (IOException ex)
        {
            throw new DataStoreException($"Could not read '{_path}'.", ex);
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var transactions = JsonSerializer.Deserialize<List<Transaction>>(json, _options);
            return transactions ?? [];
        }
        catch (JsonException ex)
        {
            throw new DataStoreException($"'{_path}' is corrupt or not valid JSON.", ex);
        }
    }

    /// <inheritdoc />
    /// <exception cref="DataStoreException">The file could not be written.</exception>
    public void Save(IEnumerable<Transaction> transactions)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = _path + ".tmp";

        try
        {
            var json = JsonSerializer.Serialize(transactions, _options);
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _path, overwrite: true);
        }
        catch (IOException ex)
        {
            throw new DataStoreException($"Could not write '{_path}'.", ex);
        }
    }
}
