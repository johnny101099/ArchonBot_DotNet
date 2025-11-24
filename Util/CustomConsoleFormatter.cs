using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace ArchonBot.Util
{
    public class CustomConsoleFormatter : ConsoleFormatter
    {
        public CustomConsoleFormatter() : base("custom") { }
        // ANSI Colors
        private const string Reset = "\u001b[0m";
        private const string DarkRed = "\u001b[31m";
        private const string DarkGreen = "\u001b[32m";
        private const string DarkYellow = "\u001b[33m";
        private const string DarkBlue = "\u001b[34m";
        private const string DarkPurple = "\u001b[35m";
        private const string DarkCyan = "\u001b[36m";
        private const string Gray = "\u001b[37m";       //  實際上是暗色的白
        private const string DarkGray = "\u001b[90m";   //  實際上是亮色的黑
        private const string Red = "\u001b[91m";
        private const string Green = "\u001b[92m";
        private const string Yellow = "\u001b[93m";
        private const string Blue = "\u001b[94m";
        private const string Purple = "\u001b[95m";
        private const string Cyan = "\u001b[96m";
        private const string White = "\u001b[97m";
        private const string Indent = "                    ";

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider,
            TextWriter textWriter)
        {
            var timestamp = $"{DarkGray}{DateTime.Now:yyyy-MM-dd HH:mm:ss}{Reset}";
            var level = GetLevelTag(logEntry.LogLevel);
            var category = $"{Green}{logEntry.Category}{Reset}";
            var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
            var msg = $"{White}{message?.Replace("\n", $"\n{Indent}")}{Reset}\n";
            textWriter.Write($"{timestamp} {level} {category} :\n{Indent}{msg}");
        }

        private static string GetLevelTag(LogLevel level) =>
            level switch
            {
                LogLevel.Trace => $"{Gray}[追蹤]{Reset}",       // Grey
                LogLevel.Debug => $"{Blue}[除錯]",
                LogLevel.Information => $"{Cyan}[資訊]", // Green
                LogLevel.Warning => $"{DarkYellow}[警告]",     // Yellow
                LogLevel.Error => $"{Red}[錯誤]",       // Red
                LogLevel.Critical => $"{Purple}[嚴重]",
                _ => White
            };
    }

}
