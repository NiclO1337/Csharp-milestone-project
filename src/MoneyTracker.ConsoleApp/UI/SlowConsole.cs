using System.Text;
using System.Threading;

namespace MoneyTracker.ConsoleApp.UI;

/// <summary>
/// Paces every line of console output with a small delay, so screens fill in gradually instead
/// of dumping onto the terminal instantly. Installed once at startup — nothing else in the app
/// needs to know it exists, or hold any state on its behalf.
/// </summary>
internal static class SlowConsole
{
    private const int RowDelayMs = 30;

    internal static void Install() => Console.SetOut(new SlowWriter(Console.Out));

    private sealed class SlowWriter(TextWriter inner) : TextWriter
    {
        public override Encoding Encoding => inner.Encoding;

        public override void Write(char value) => inner.Write(value);

        public override void Write(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var segments = value.Split('\n');
            for (var i = 0; i < segments.Length; i++)
            {
                inner.Write(segments[i]);
                if (i < segments.Length - 1)
                {
                    inner.Write('\n');
                    Thread.Sleep(RowDelayMs);
                }
            }
        }

        public override void WriteLine(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                inner.WriteLine();
                Thread.Sleep(RowDelayMs);
                return;
            }

            foreach (var line in value.Split('\n'))
            {
                inner.WriteLine(line);
                Thread.Sleep(RowDelayMs);
            }
        }

        public override void WriteLine()
        {
            inner.WriteLine();
            Thread.Sleep(RowDelayMs);
        }
    }
}
