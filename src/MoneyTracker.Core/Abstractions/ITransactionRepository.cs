using MoneyTracker.Core.Exceptions;
using MoneyTracker.Core.Models;

namespace MoneyTracker.Core.Abstractions;

/// <summary>
/// Persistence contract for transactions. Deliberately minimal, so swapping the storage
/// mechanism means writing one new implementation, not changing the domain.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>Loads every stored transaction. A missing or empty store returns an empty list.</summary>
    /// <exception cref="DataStoreException">The store exists but could not be read.</exception>
    IReadOnlyList<Transaction> Load();

    /// <summary>Replaces the entire stored transaction list with <paramref name="transactions"/>.</summary>
    void Save(IEnumerable<Transaction> transactions);
}
