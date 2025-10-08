using static MockHttp.BitRate;

namespace MockHttp;

public sealed class BitRateTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Given_that_value_is_invalid_when_casting_it_should_throw(int bitRate)
    {
        // ACt
        Func<BitRate> act = () => (BitRate)bitRate;

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(bitRate));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Given_that_value_is_invalid_when_converting_it_should_throw(int bitRate)
    {
        // ACt
        Func<BitRate> act = () => FromInt32(bitRate);

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(bitRate));
    }

    [Theory]
    [InlineData(1_000)]
    [InlineData(1_234_567)]
    public void When_converting_to_int_it_should_return_expected(int bitRate)
    {
        BitRate sut = FromInt32(bitRate);

        // Act
        int actual = ToInt32(sut);

        // Assert
        actual.Should().Be(bitRate);
    }

    [Theory]
    [InlineData(1_000)]
    [InlineData(1_234_567)]
    public void When_casting_to_int_it_should_return_expected(int bitRate)
    {
        BitRate sut = FromInt32(bitRate);

        // Act
        int actual = (int)sut;

        // Assert
        actual.Should().Be(bitRate);
    }

    [Theory]
    [MemberData(nameof(GetBitRateTestCases))]
    public void When_creating_using_builtin_factories_it_should_return_expected(BitRate bitRate, int expectedBitRate)
    {
        ToInt32(bitRate).Should().Be(expectedBitRate);
    }

    public static IEnumerable<object[]> GetBitRateTestCases()
    {
        yield return [TwoG(), 64_000];
        yield return [ThreeG(), 2_000_000];
        yield return [FourG(), 64_000_000];
        yield return [FiveG(), 512_000_000];
        yield return [TenMegabit(), 10_000_000];
        yield return [OneHundredMegabit(), 100_000_000];
        yield return [OneGigabit(), 1_000_000_000];
        yield return [FromInt32(999), 999];
        yield return [FromInt32(1_000), 1_000];
        yield return [FromInt32(1_234), 1_234];
        yield return [FromInt32(1_000_000), 1_000_000];
        yield return [FromInt32(1_234_567), 1_234_567];
        yield return [FromInt32(1_000_000_000), 1_000_000_000];
        yield return [FromInt32(1_234_567_890), 1_234_567_890];
    }

    [Theory]
    [MemberData(nameof(GetBitRatePrettyTextTestCases))]
    public void When_formatting_it_should_return_expected(BitRate bitRate, string expectedPrettyText)
    {
        bitRate.ToString().Should().Be(expectedPrettyText);
    }

    public static IEnumerable<object[]> GetBitRatePrettyTextTestCases()
    {
        const string prefix = nameof(BitRate) + ".";
        yield return [TwoG(), prefix + nameof(TwoG)];
        yield return [ThreeG(), prefix + nameof(ThreeG)];
        yield return [FourG(), prefix + nameof(FourG)];
        yield return [FiveG(), prefix + nameof(FiveG)];
        yield return [TenMegabit(), prefix + nameof(TenMegabit)];
        yield return [OneHundredMegabit(), prefix + nameof(OneHundredMegabit)];
        yield return [OneGigabit(), prefix + nameof(OneGigabit)];
        yield return [FromInt32(999), prefix + "Around(999bps)"];
        yield return [FromInt32(1_000), prefix + "Around(1kbps)"];
        yield return [FromInt32(1_234), prefix + "Around(1.23kbps)"];
        yield return [FromInt32(1_000_000), prefix + "Around(1Mbps)"];
        yield return [FromInt32(1_234_567), prefix + "Around(1.23Mbps)"];
        yield return [FromInt32(1_000_000_000), prefix + "Around(1Gbps)"];
        yield return [FromInt32(1_234_567_890), prefix + "Around(1.23Gbps)"];
    }
}
