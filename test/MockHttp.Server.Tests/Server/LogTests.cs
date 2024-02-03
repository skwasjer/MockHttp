using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MockHttp.Server;

public sealed class LogTests
{
    [Fact]
    public void Log_should_produce_expected_output()
    {
        ILogger? logger = Substitute.For<ILogger>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        string traceId = Guid.NewGuid().ToString("D");
        string connId = Guid.NewGuid().ToString("D");
        var httpContext = new DefaultHttpContext { TraceIdentifier = traceId, Connection = { Id = connId } };

        const string customMessage = "hello world";
        var ex = new InvalidOperationException();

        Func<IReadOnlyList<KeyValuePair<string, object>>, bool> assertState = state =>
        {
            state.Should().ContainKey("{OriginalFormat}").WhoseValue.Should().Be(Log.LogRequestMessageTemplate);
            state.Should().ContainKey("RequestId").WhoseValue.Should().Be(traceId);
            state.Should().ContainKey("ConnectionId").WhoseValue.Should().Be(connId);
            state.Should().ContainKey("Message").WhoseValue.Should().Be(customMessage);
            return true;
        };

        // Act
        logger.LogRequestMessage(httpContext, customMessage, ex);

        // Assert
        logger.Received(1)
            .Log(
                LogLevel.Debug,
                Arg.Is<EventId>(e => e.Id == 0),
                Arg.Is<IReadOnlyList<KeyValuePair<string, object>>>(state => assertState(state)),
                ex,
                Arg.Any<Func<IReadOnlyList<KeyValuePair<string, object>>, Exception?, string>>()
            );
    }
}
