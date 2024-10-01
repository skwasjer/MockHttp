using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MockHttp.Server;

internal static partial class Log
{
    internal const string LogRequestMessageTemplate = "Connection id \"{ConnectionId}\", Request id \"{RequestId}\": {Message}";

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Debug,
        Message = LogRequestMessageTemplate
    )]
    private static partial void LogDebugRequestMessage(
        ILogger logger,
        string connectionId,
        string requestId,
        string message,
        Exception? exception
    );

    public static void LogRequestMessage(this ILogger logger, HttpContext httpContext, string message, Exception? exception = null)
    {
        LogDebugRequestMessage(logger, httpContext.Connection.Id, httpContext.TraceIdentifier, message, exception);
    }
}
