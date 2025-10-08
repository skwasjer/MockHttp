using System.Diagnostics;
using Xunit.Abstractions;
using static MockHttp.NetworkLatency;

namespace MockHttp;

public sealed class NetworkLatencyTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public NetworkLatencyTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [MemberData(nameof(GetNetworkLatencyTestCases))]
    public async Task Given_that_latency_is_configured_when_simulating_it_should_delay_for_expected_time
    (
        NetworkLatency networkLatency,
        int minExpectedDelayInMs,
        int maxExpectedDelayInMs
    )
    {
        bool isDelegateCalled = false;

        // Act
        var sw = Stopwatch.StartNew();
        TimeSpan simulatedLatency = await networkLatency.SimulateAsync(() =>
        {
            isDelegateCalled = true;
            return Task.CompletedTask;
        });
        TimeSpan executionTime = sw.Elapsed;

        // Assert
        _testOutputHelper.WriteLine(networkLatency.ToString());
        _testOutputHelper.WriteLine("Random simulated latency : {0}", simulatedLatency);
        _testOutputHelper.WriteLine("Total execution time     : {0}", executionTime);

        simulatedLatency
            .Should()
            .BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(minExpectedDelayInMs))
            .And.BeLessThanOrEqualTo(TimeSpan.FromMilliseconds(maxExpectedDelayInMs));
        isDelegateCalled.Should().BeTrue();
    }

    public static IEnumerable<object[]> GetNetworkLatencyTestCases()
    {
        yield return [TwoG(), 300, 1200];
        yield return [ThreeG(), 100, 600];
        yield return [FourG(), 30, 50];
        yield return [FiveG(), 5, 10];
        yield return [Between(50, 60), 50, 60];
        yield return [Between(TimeSpan.FromMilliseconds(2500), TimeSpan.FromMilliseconds(3000)), 2500, 3000];
        yield return [Around(100), 100, 100];
        yield return [Around(TimeSpan.FromMilliseconds(70)), 70, 70];
    }

    [Theory]
    [MemberData(nameof(GetNetworkLatencyPrettyTextTestCases))]
    public void Given_that_latency_is_configured_when_formatting_it_should_return_expected(NetworkLatency networkLatency, string expectedPrettyText)
    {
        networkLatency.ToString().Should().Be(expectedPrettyText);
    }

    public static IEnumerable<object[]> GetNetworkLatencyPrettyTextTestCases()
    {
        const string prefix = nameof(NetworkLatency) + ".";
        yield return [TwoG(), prefix + nameof(TwoG)];
        yield return [ThreeG(), prefix + nameof(ThreeG)];
        yield return [FourG(), prefix + nameof(FourG)];
        yield return [FiveG(), prefix + nameof(FiveG)];
        yield return [Between(50, 60), prefix + "Between(50, 60)"];
        yield return
        [
            Between(TimeSpan.FromMilliseconds(150), TimeSpan.FromMilliseconds(160)), prefix + "Between(00:00:00.1500000, 00:00:00.1600000)"
        ];
        yield return [Around(100), prefix + "Around(100)"];
        yield return [Around(TimeSpan.FromMilliseconds(70)), prefix + "Around(00:00:00.0700000)"];
    }
}
