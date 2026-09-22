namespace MoneyTracker.ConsoleApp.UI;

/// <summary>
/// Signals that the current input flow cannot continue: the user typed "q", or the input stream
/// itself is closed (<see cref="Console.ReadLine"/> returned <see langword="null"/>, e.g. piped
/// or redirected input reaching its end). Caught by <see cref="ConsoleInput.TryRun"/> so this
/// unwinds to the previous menu instead of looping forever or propagating as an unhandled error.
/// </summary>
internal sealed class UserCancelledException : Exception
{
}
