using MoneyTracker.ConsoleApp.UI;
using MoneyTracker.Core.Exceptions;
using MoneyTracker.Core.Services;
using MoneyTracker.Infrastructure.Json;

SlowConsole.Install();

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
    ConsoleMessage.WriteColoredLine(line, ConsoleColor.DarkYellow);
}

ConsoleMessage.WriteColoredSlow("\nWelcome to Dragon's Ledger - where every coin counts.\n", ConsoleColor.DarkYellow);
ConsoleMessage.WriteColoredSlow("\nA treasure-keeper's ledger for tracking your gold:\n", ConsoleColor.DarkYellow);
ConsoleMessage.WriteColoredSlow("mind your mint, weigh your winnings, spot the coin\n", ConsoleColor.DarkYellow);
ConsoleMessage.WriteColoredSlow("that got away, and keep your hoard from gathering dust.\n", ConsoleColor.DarkYellow);
ConsoleInput.Pause();

// AppContext.BaseDirectory is bin/Debug/net10.0/; walk back up to src/ so the data file
// lives in the source tree and gets committed, not left behind in the gitignored build output.
var path = Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..", "..",
    "MoneyTracker.Infrastructure", "data", "transactions.json"));
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

ConsoleMessage.WriteColoredSlow("The hoard is secure and the ledger is closed... for now.\n\n", ConsoleColor.DarkYellow);
ConsoleMessage.WriteColoredSlow("Farewell, treasure keeper!\n\n\n", ConsoleColor.DarkYellow);

foreach (string line in new[]
{
    "                        \\`-\\`-._",
    "                         \\` )`. `-.__      ,",
    "      '' , . _       _,-._;'_,-`__,-'    ,/",
    "     : `. ` , _' :- '--'._ ' `------._,-;'",
    "      `- ,`- '            `--..__,,---'   hh",
})
{
    ConsoleMessage.WriteColoredLine(line, ConsoleColor.DarkYellow);
}
ConsoleMessage.NewLine();