using Chi.Shared;
using System.Collections.Concurrent;

namespace Chi
{
    public static class Output
    {
        #region Background Output Logic

        class OutputItem
        {
            public string Text { get; set; } = string.Empty;
            public ConsoleColor ForegroundColor { get; set; }
            public ConsoleColor BackgroundColor { get; set; }
        }

        class ExitSignal : OutputItem { }

        static EventWaitHandle WriteWaitHandle { get; } = new(false, EventResetMode.AutoReset);
        static ConcurrentQueue<OutputItem> Queue { get; } = new();

        static Output()
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    WriteWaitHandle.WaitOne();

                    while (Queue.TryDequeue(out var item))
                    {
                        Print(item.Text, item.ForegroundColor, item.BackgroundColor);

                        if (item is ExitSignal)
                        {
                            // Signal the main thread that the Output thread
                            // has written all the output and is ready to terminate.
                            Entry.OutputWaitHandle.Set();
                            return;
                        }
                    }
                }
            })
            { IsBackground = true };

            thread.Start();
        }

        static void Print(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            var defaultForeground = Console.ForegroundColor;
            var defaultBackground = Console.BackgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
            Console.ForegroundColor = defaultForeground;
            Console.BackgroundColor = defaultBackground;
        }

        #endregion

        #region Background Output Methods

        public static void WriteLine(ConsoleColor? foregroundColor = default, ConsoleColor? backgroundColor = default) =>
            WriteLine(string.Empty, foregroundColor, backgroundColor);

        public static void WriteLine(string text, ConsoleColor? foregroundColor = default, ConsoleColor? backgroundColor = default)
        {
            Write(text, foregroundColor, backgroundColor);
            Write(Environment.NewLine, default, default);
        }

        public static void Write(string text, ConsoleColor? foregroundColor = default, ConsoleColor? backgroundColor = default)
        {
            foregroundColor ??= Settings.DefaultForegroundColor;
            backgroundColor ??= Settings.DefaultBackgroundColor;

            Queue.Enqueue(new OutputItem
            {
                Text = text,
                ForegroundColor = foregroundColor.Value,
                BackgroundColor = backgroundColor.Value,
            });
            WriteWaitHandle.Set();
        }

        internal static void SignalExit()
        {
            Queue.Enqueue(new ExitSignal());
            WriteWaitHandle.Set();
        }

        #endregion
    }
}
