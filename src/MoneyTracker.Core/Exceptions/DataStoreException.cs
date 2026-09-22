namespace MoneyTracker.Core.Exceptions;

/// <summary>
/// Thrown when the transaction store exists but could not be read or written — a corrupt or
/// otherwise unreadable file, never a missing one. Wraps the original <see cref="Exception"/>
/// (typically a <see cref="System.Text.Json.JsonException"/> or <see cref="IOException"/>).
/// </summary>
public sealed class DataStoreException : Exception
{
    /// <summary>Creates a new <see cref="DataStoreException"/> wrapping <paramref name="innerException"/>.</summary>
    public DataStoreException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
