using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace MockHttp.Fixtures;

public delegate void CaptureDelegate(string message);

public class CapturingLoggerFactoryFixture : LoggerFactoryFixture
{
    private static readonly AsyncLocal<LogContext?> LogContextLocal = new();

    public CapturingLoggerFactoryFixture()
        : base(configure => configure
            .AddConsole(opts => opts.FormatterName = ConsoleCapture.NameKey)
            .AddConsoleFormatter<ConsoleCapture, ConsoleCaptureOptions>(opts => opts.IncludeScopes = true)
            .Services.AddSingleton((CaptureDelegate)(message => LogContextLocal.Value?.Events.Add(message)))
        )
    {
    }

    public static LogContext CreateContext()
    {
        return LogContextLocal.Value = new LogContext(() => LogContextLocal.Value = null);
    }

    public sealed class LogContext(Action dispose) : IDisposable
    {
        public List<string> Events { get; } = new();

        public void Dispose()
        {
            dispose();
        }
    }

    private class ConsoleCapture : ConsoleFormatter
    {
        internal const string NameKey = "console-capture";
        private const string LogLevelPadding = ": ";
        private static readonly string MessagePadding = new(' ', GetLogLevelString(LogLevel.Information).Length + LogLevelPadding.Length);
        private static readonly string NewLineWithMessagePadding = Environment.NewLine + MessagePadding;

        private readonly CaptureDelegate _onWrite;

        private readonly ConsoleCaptureOptions _options;

        public ConsoleCapture
        (
            CaptureDelegate onWrite,
            IOptions<ConsoleCaptureOptions> options
        ) : base(NameKey)
        {
            _onWrite = onWrite ?? throw new ArgumentNullException(nameof(onWrite));
            _options = options.Value;
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            var sb = new StringBuilder();
            textWriter = new StringWriter(sb);

            string? message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception is null && message is null)
            {
                return;
            }

            LogLevel logLevel = logEntry.LogLevel;
            string logLevelString = GetLogLevelString(logLevel);

            string? timestamp = null;
            string? timestampFormat = _options.TimestampFormat;
            if (timestampFormat is not null)
            {
                DateTimeOffset dateTimeOffset = GetCurrentDateTime();
                timestamp = dateTimeOffset.ToString(timestampFormat);
            }

            if (!string.IsNullOrEmpty(timestamp))
            {
                textWriter.Write(timestamp);
            }

            if (!string.IsNullOrEmpty(logLevelString))
            {
                textWriter.Write(logLevelString);
            }

            CreateDefaultLogMessage(textWriter, in logEntry, message, scopeProvider);

            _onWrite(sb.ToString());
        }

        private void CreateDefaultLogMessage<TState>(TextWriter textWriter, in LogEntry<TState> logEntry, string message, IExternalScopeProvider? scopeProvider)
        {
            bool singleLine = _options.SingleLine;
            int eventId = logEntry.EventId.Id;
            Exception? exception = logEntry.Exception;

            // Example:
            // info: ConsoleApp.Program[10]
            //       Request received

            // category and event id
            textWriter.Write(LogLevelPadding);
            textWriter.Write(logEntry.Category);
            textWriter.Write('[');

#if NETCOREAPP
            Span<char> span = stackalloc char[10];
            if (eventId.TryFormat(span, out int charsWritten))
            {
                textWriter.Write(span.Slice(0, charsWritten));
            }
            else
#endif
            {
                textWriter.Write(eventId.ToString());
            }

            textWriter.Write(']');
            if (!singleLine)
            {
                textWriter.Write(Environment.NewLine);
            }

            // scope information
            WriteScopeInformation(textWriter, scopeProvider, singleLine);
            WriteMessage(textWriter, message, singleLine);

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (exception is not null)
            {
                // exception message
                WriteMessage(textWriter, exception.ToString(), singleLine);
            }

            if (singleLine)
            {
                textWriter.Write(Environment.NewLine);
            }
        }

        private static void WriteMessage(TextWriter textWriter, string message, bool singleLine)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (singleLine)
                {
                    textWriter.Write(' ');
                    WriteReplacing(textWriter, Environment.NewLine, " ", message);
                }
                else
                {
                    textWriter.Write(MessagePadding);
                    WriteReplacing(textWriter, Environment.NewLine, NewLineWithMessagePadding, message);
                    textWriter.Write(Environment.NewLine);
                }
            }

            static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
            {
                string newMessage = message.Replace(oldValue, newValue);
                writer.Write(newMessage);
            }
        }

        private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider? scopeProvider, bool singleLine)
        {
            if (_options.IncludeScopes && scopeProvider is not null)
            {
                bool paddingNeeded = !singleLine;
                scopeProvider.ForEachScope((scope, state) =>
                    {
                        if (paddingNeeded)
                        {
                            paddingNeeded = false;
                            state.Write(MessagePadding);
                            state.Write("=> ");
                        }
                        else
                        {
                            state.Write(" => ");
                        }

                        state.Write(scope);
                    },
                    textWriter);

                if (!paddingNeeded && !singleLine)
                {
                    textWriter.Write(Environment.NewLine);
                }
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }

        private DateTimeOffset GetCurrentDateTime()
        {
            return _options.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
        }
    }

    private class ConsoleCaptureOptions : ConsoleFormatterOptions
    {
        /// <summary>
        /// When <see langword="true" />, the entire message gets logged in a single line.
        /// </summary>
        public bool SingleLine { get; set; }
    }
}
