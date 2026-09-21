namespace MoneyTracker.ConsoleApp.UI;

/// <summary>
/// Signals that the user cancelled the current input flow (typed "q"). Caught by
/// <see cref="ConsoleInput.TryRun"/> so cancellation returns to the previous menu instead of
/// propagating as an unhandled error.
/// </summary>
internal sealed class UserCancelledException : Exception
{
}
