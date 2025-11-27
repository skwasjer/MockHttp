using MockHttp.Matchers;

namespace MockHttp.Request;

public class AnyRequestMatchingTests
{
    private readonly AnyRequestMatching _sut;
    private readonly HttpRequestMatcher _matcher1;
    private readonly HttpRequestMatcher _matcher2;
    private bool _isExclusive1;
    private bool _isExclusive2;

    public AnyRequestMatchingTests()
    {
        _sut = new AnyRequestMatching();

        _matcher1 = CreateMatcherMock(() => _isExclusive1);
        _matcher2 = CreateMatcherMock(() => _isExclusive2);
        return;

        static HttpRequestMatcher CreateMatcherMock(Func<bool> returns)
        {
            HttpRequestMatcher matcherMock = Substitute.For<HttpRequestMatcher>();
            matcherMock.IsExclusive.Returns(_ => returns());
            return matcherMock;
        }
    }

    [Fact]
    public void Given_two_mutually_exclusive_instances_of_same_type_when_adding_should_not_throw()
    {
        _isExclusive1 = true;
        _isExclusive2 = true;
        _sut.Add(_matcher1);

        // Act
        Action act = () => _sut.Add(_matcher2);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Given_two_instances_of_same_type_of_which_first_is_exclusive_when_adding_second_should_not_throw()
    {
        _isExclusive1 = true;
        _isExclusive2 = false;
        _sut.Add(_matcher1);

        // Act
        Action act = () => _sut.Add(_matcher2);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Given_two_instances_of_same_type_of_which_second_is_exclusive_when_adding_second_should_not_throw()
    {
        _isExclusive1 = false;
        _isExclusive2 = true;
        _sut.Add(_matcher1);

        // Act
        Action act = () => _sut.Add(_matcher2);

        // Assert
        act.Should().NotThrow();
    }
}
