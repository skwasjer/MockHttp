using FluentAssertions;
using MockHttp.Matchers;
using Moq;
using Xunit;

namespace MockHttp;

public class AnyRequestMatchingTests
{
    private readonly RequestMatching _sut;
    private readonly HttpRequestMatcher _matcher1;
    private readonly HttpRequestMatcher _matcher2;
    private bool _isExclusive1;
    private bool _isExclusive2;

    public AnyRequestMatchingTests()
    {
        static HttpRequestMatcher CreateMatcherMock(Func<bool> returns)
        {
            var matcherMock = new Mock<HttpRequestMatcher>();
            matcherMock
                .Setup(m => m.IsExclusive)
                .Returns(returns);
            return matcherMock.Object;
        }

        _sut = new AnyRequestMatching();

        _matcher1 = CreateMatcherMock(() => _isExclusive1);
        _matcher2 = CreateMatcherMock(() => _isExclusive2);
    }

    [Fact]
    public void Given_two_mutually_exclusive_instances_of_same_type_when_adding_should_not_throw()
    {
        _isExclusive1 = true;
        _isExclusive2 = true;
        _sut.With(_matcher1);

        // Act
        Action act = () => _sut.With(_matcher2);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Given_two_instances_of_same_type_of_which_first_is_exclusive_when_adding_second_should_not_throw()
    {
        _isExclusive1 = true;
        _isExclusive2 = false;
        _sut.With(_matcher1);

        // Act
        Action act = () => _sut.With(_matcher2);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Given_two_instances_of_same_type_of_which_second_is_exclusive_when_adding_second_should_not_throw()
    {
        _isExclusive1 = false;
        _isExclusive2 = true;
        _sut.With(_matcher1);

        // Act
        Action act = () => _sut.With(_matcher2);

        // Assert
        act.Should().NotThrow();
    }
}
