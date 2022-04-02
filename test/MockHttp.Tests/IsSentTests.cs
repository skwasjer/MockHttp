using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp;

public static class IsSentTests
{
    public class AtLeast
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Given_call_count_below_1_when_creating_should_throw(int callCount)
        {
            Action act = () => IsSent.AtLeast(callCount);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(nameof(callCount));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Given_call_count_above_0_when_creating_should_not_throw(int callCount)
        {
            Action act = () => IsSent.AtLeast(callCount);

            // Assert
            act.Should().NotThrow<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(1, 0, false)]
        [InlineData(1, 1, true)]
        [InlineData(1, 100, true)]
        [InlineData(int.MaxValue, int.MaxValue, true)]
        public void When_verifying_at_or_above_call_count_should_be_true(int callCount, int verifyCount, bool expected)
        {
            var atLeast = IsSent.AtLeast(callCount);

            // Assert
            atLeast.Verify(verifyCount).Should().Be(expected);
        }
    }

    public class AtLeastOnce
    {
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(int.MaxValue, true)]
        public void When_verifying_at_or_above_call_count_should_be_true(int verifyCount, bool expected)
        {
            var atLeastOnce = IsSent.AtLeastOnce();

            // Assert
            atLeastOnce.Verify(verifyCount).Should().Be(expected);
        }
    }

    public class AtMost
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Given_call_count_below_0_when_creating_should_throw(int callCount)
        {
            Action act = () => IsSent.AtMost(callCount);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(nameof(callCount));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Given_call_count_at_or_above_0_when_creating_should_not_throw(int callCount)
        {
            Action act = () => IsSent.AtMost(callCount);

            // Assert
            act.Should().NotThrow<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(1, 0, true)]
        [InlineData(1, 1, true)]
        [InlineData(1, 100, false)]
        [InlineData(int.MaxValue - 1, int.MaxValue, false)]
        [InlineData(int.MaxValue, int.MaxValue, true)]
        public void When_verifying_at_or_below_call_count_should_be_true(int callCount, int verifyCount, bool expected)
        {
            var atMost = IsSent.AtMost(callCount);

            // Assert
            atMost.Verify(verifyCount).Should().Be(expected);
        }
    }

    public class AtMostOnce
    {
        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(100, false)]
        [InlineData(int.MaxValue, false)]
        public void When_verifying_at_or_below_call_count_should_be_true(int verifyCount, bool expected)
        {
            var atLeastOnce = IsSent.AtMostOnce();

            // Assert
            atLeastOnce.Verify(verifyCount).Should().Be(expected);
        }
    }

    public class Exactly
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        public void Given_call_count_below_0_when_creating_should_throw(int callCount)
        {
            Action act = () => IsSent.Exactly(callCount);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(nameof(callCount));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Given_call_count_above_or_equal_to_0_when_creating_should_not_throw(int callCount)
        {
            Action act = () => IsSent.Exactly(callCount);

            // Assert
            act.Should().NotThrow<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(0, 1, false)]
        [InlineData(0, 0, true)]
        [InlineData(1, 0, false)]
        [InlineData(0, 100, false)]
        [InlineData(100, 100, true)]
        [InlineData(100, 0, false)]
        public void When_verifying_above_or_below_call_count_should_be_false(int callCount, int verifyCount, bool expected)
        {
            var exactly = IsSent.Exactly(callCount);

            // Assert
            exactly.Verify(verifyCount).Should().Be(expected);
        }
    }

    public class Never
    {
        [Theory]
        [InlineData(1, false)]
        [InlineData(0, true)]
        [InlineData(100, false)]
        public void When_verifying_more_than_0_should_be_false(int verifyCount, bool expected)
        {
            var never = IsSent.Never();

            // Assert
            never.Verify(verifyCount).Should().Be(expected);
        }
    }

    public class Once
    {
        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        [InlineData(100, false)]
        [InlineData(int.MaxValue, false)]
        public void When_verifying_anything_other_than_1_should_be_false(int verifyCount, bool expected)
        {
            var once = IsSent.Once();

            // Assert
            once.Verify(verifyCount).Should().Be(expected);
        }
    }
}
