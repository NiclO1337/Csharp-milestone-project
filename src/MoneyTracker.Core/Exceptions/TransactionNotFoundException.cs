namespace MoneyTracker.Core.Exceptions;

/// <summary>Thrown when no transaction exists with the requested ID.</summary>
public sealed class TransactionNotFoundException : Exception
{
    /// <summary>The ID that could not be found.</summary>
    public int Id { get; }

    /// <summary>Creates a new <see cref="TransactionNotFoundException"/> for the given <paramref name="id"/>.</summary>
    public TransactionNotFoundException(int id)
        : base($"No transaction found with ID {id}.")
    {
        Id = id;
    }
}
