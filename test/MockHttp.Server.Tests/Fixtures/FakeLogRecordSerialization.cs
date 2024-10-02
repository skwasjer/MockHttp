using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace MockHttp.Fixtures;

internal static class FakeLogRecordSerialization
{
    internal static string Serialize(FakeLogRecord e)
    {
        var sb = new StringBuilder();
        const int len = 4;
        string indent = new(' ', len + 2);

        sb.AppendLine($"{GetLogLevelString(e.Level)}: {e.Category}[{e.Id.Id}]");
        foreach (IEnumerable<KeyValuePair<string, object?>> scope in e.Scopes.OfType<IEnumerable<KeyValuePair<string, object?>>>())
        {
            sb.Append(indent);
            // ReSharper disable once UsageOfDefaultStructEquality
            foreach (KeyValuePair<string, object?> kvp in scope)
            {
                sb.Append($"=> {kvp} ");
            }

            sb.AppendLine();
        }

        sb.Append(indent);
        sb.AppendLine(e.Message);

        if (e.Exception is not null)
        {
            sb.Append(indent);
            sb.AppendLine(e.Exception.ToString());
        }

        return sb.ToString();
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
}
