using MoneyTracker.ConsoleApp.UI;
using MoneyTracker.Core.Exceptions;
using MoneyTracker.Core.Services;
using MoneyTracker.Infrastructure.Json;
Console.ForegroundColor = ConsoleColor.DarkYellow;
foreach (string line in new[]
{
    "",
    "                                               _   __,----'~~~~~~~~~`-----.__",
    "                                        .  .    `//====-              ____,-'~`",
    "                        -.            \\_|// .   /||\\\\  `~~~~`---.___./",
    "                  ______-==.       _-~o  `\\/    |||  \\\\           _,'`",
    "            __,--'   ,=='||\\=_    ;_,_,/ _-'|-   |`\\   \\\\        ,'",
    "         _-'      ,='    | \\\\`.    '',/~7  /-   /  ||   `\\.     /",
    "       .'       ,'       |  \\\\  \\_  \"  /  /-   /   ||      \\   /",
    "      / _____  /         |     \\\\.`-_/  /|- _/   ,||       \\ /",
    "     ,-'     `-|--'~~`--_ \\     `==-/  `| \\'--===-'       _/`",
    "               '         `-|      /|    )-'\\~'      _,--\"'",
    "                           '-~^\\_/ |    |   `\\_   ,^             /\\",
    "                                /  \\     \\__   \\/~               `\\__",
    "                            _,-' _/'\\ ,-'~____-'`-/                 ``===\\",
    "                           ((->/'    \\|||' `.     `\\.  ,                _||",
    "             ./                       \\_     `\\      `~---|__i__i__\\--~'_/",
    "            <_n_                     __-^-_    `)  \\-.______________,-~'",
    "             `B'\\)                  ///,-'~`__--^-  |-------~~~~^'",
    "             /^>                           ///,--~`-\\",
    "            `  `                                       -Tua Xiong",
})
{
    Console.WriteLine(line);
}

Console.WriteLine("\nWelcome to Dragon's Ledger - where every coin counts.\n");
Console.WriteLine("A treasure-keeper's ledger for tracking your gold:");
Console.WriteLine("mind your mint, weigh your winnings, spot the coin");
Console.WriteLine("that got away, and keep your hoard from gathering dust.\n");
Console.ResetColor();

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

ConsoleMessage.Heading("Closing application");

Console.ForegroundColor = ConsoleColor.DarkYellow;
Console.WriteLine("The hoard is secure and the ledger is closed... for now.\n");
Console.WriteLine("Farewell, treasure keeper!\n\n");

foreach (string line in new[]
{
    "                        \\`-\\`-._",
    "                         \\` )`. `-.__      ,",
    "      '' , . _       _,-._;'_,-`__,-'    ,/",
    "     : `. ` , _' :- '--'._ ' `------._,-;'",
    "      `- ,`- '            `--..__,,---'   hh",
})
{
    Console.WriteLine(line);
}
Console.ResetColor();
Console.WriteLine();