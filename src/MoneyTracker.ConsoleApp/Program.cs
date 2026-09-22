using MoneyTracker.ConsoleApp.UI;
using MoneyTracker.Core.Exceptions;
using MoneyTracker.Core.Services;
using MoneyTracker.Infrastructure.Json;

var path = Path.Combine(AppContext.BaseDirectory, "data", "transactions.json");
var repository = new JsonTransactionRepository(path);

try
{
    var service = new TransactionService(repository);
    new MainMenu(service).Run();
}
catch (DataStoreException ex)
{
    ConsoleMessage.DisplayErrorMessage($"Could not load transaction data: {ex.Message}");
}
