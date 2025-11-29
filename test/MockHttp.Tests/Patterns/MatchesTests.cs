using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace MockHttp.Patterns;

public abstract class MatchesTests
{
    public sealed class InstanceTests : MatchesTests
    {
        [Fact]
        public void Given_that_value_is_null_when_initializing_it_should_throw()
        {
            string? value = null;

            // Act
            Func<Matches> act = () => new Matches
            {
                Value = value!,
                IsMatch = _ => true
            };

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(value));
        }

        [Fact]
        public void Given_that_isMatch_delegate_is_null_when_initializing_it_should_throw()
        {
            Func<string, bool>? value = null;

            // Act
            Func<Matches> act = () => new Matches
            {
                Value = string.Empty,
                IsMatch = value!
            };

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(value));
        }
    }

    public sealed class EmptyTests : MatchesTests
    {
        [Fact]
        public void When_uninitialized_it_should_be_empty()
        {
#pragma warning disable S1481
#pragma warning disable CS8887
            Matches sut;

            sut.Value.Should()
                .BeSameAs(Matches.Empty.Value)
                .And.BeSameAs(Matches.Empty.ToString())
                .And.BeSameAs(nameof(Matches.Empty));
            sut.IsMatch.Should().BeSameAs(Matches.Empty.IsMatch);
            sut.Should().BeEquivalentTo(Matches.Empty);
#pragma warning restore CS8887
#pragma warning restore S1481
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("123", false)]
        public void It_should_only_match_null_or_empty_strings(string? value, bool shouldMatch)
        {
            Matches.Empty.IsMatch(value!).Should().Be(shouldMatch);
        }
    }

    public sealed class AnyTests : MatchesTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData("abcdef")]
        public void It_should_always_match(string value)
        {
            Matches.Any.IsMatch(value).Should().BeTrue();
        }

        [Fact]
        public void It_should_have_expected_value()
        {
            Matches.Any.Value.Should()
                .BeSameAs(Matches.Any.ToString())
                .And.BeSameAs(nameof(Matches.Any));
            Matches.Any.Should().BeEquivalentTo(Matches.Any);
        }
    }

    public sealed class ExactTests : MatchesTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData("abcdef")]
        public void It_should_match_exactly(string value)
        {
            Matches.Exactly(value).IsMatch(value).Should().BeTrue();
        }

        [Theory]
        [InlineData("", " ")]
        [InlineData("123", "124")]
        [InlineData("abcdef", "Abcdef")]
        public void It_should_not_match_exactly(string value, string valueToMatch)
        {
            Matches.Exactly(value).IsMatch(valueToMatch).Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData("abcdef")]
        public void It_should_have_expected_value(string value)
        {
            // Act
            var actual = Matches.Exactly(value);

            // Assert
            actual.Value.Should()
                .Be(actual.ToString())
                .And.Be(value);
        }
    }

    public sealed class WildcardTests : MatchesTests
    {
        [Fact]
        public void Given_that_value_is_null_when_creating_instance_it_should_throw()
        {
            string? value = null;

            // Act
            Func<Matches> act = () => Matches.Wildcard(value!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(value));
        }

        [Fact]
        public void Given_that_value_is_empty_when_creating_instance_it_should_throw()
        {
            string value = string.Empty;

            // Act
            Func<Matches> act = () => Matches.Wildcard(value);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName(nameof(value))
                .WithMessage("The value cannot be empty.*");
        }

        [Theory]
        [InlineData("*", ".*")]
        [InlineData("test", "^(test)$")]
        [InlineData("test*", "^(test).*")]
        [InlineData("test**", "^(test).*")]
        [InlineData("*test", ".*(test)$")]
        [InlineData("**test", ".*(test)$")]
        [InlineData("*test*", ".*(test).*")]
        [InlineData("**test**", ".*(test).*")]
        [InlineData("te*st", "^(te).+(st)$")]
        [InlineData("te**st", "^(te).+(st)$")]
        [InlineData("*test1*test2*", ".*(test1).+(test2).*")]
        [InlineData("**test1**test2**", ".*(test1).+(test2).*")]
        [InlineData("/path?*", @"^(/path\?).*")]
        [InlineData("/path/file.jpg?q=*&p=1&m=[a,b]", @"^(/path/file\.jpg\?q=).+(&p=1&m=\[a,b\])$")]
        [InlineData(@".+?^$()[]{}|\", @"^(\.\+\?\^\$\(\)\[\]\{\}\|\\)$")]
        public void When_creating_matcher_it_should_return_expected_instance(string value, string expectedRegex)
        {
            // Act
            var sut = WildcardStringMatcher.Create(value);

            // Assert
            sut.RegexMatches.Value.Should().Be(expectedRegex);
            sut.Value.Should()
                .Be(sut.ToString())
                .And.Be(value);
            sut.IsMatch.Should().NotBeNull();
        }

        [Theory]
        [InlineData("test", "test", true)]
        [InlineData("test", "testing", false)]
        [InlineData("test*", "test", true)]
        [InlineData("test*", "testing", true)]
        [InlineData("test*", "stress testing", false)]
        [InlineData("*test", "test", true)]
        [InlineData("*test", "testing", false)]
        [InlineData("*test", "stress test", true)]
        [InlineData("*test*", "test", true)]
        [InlineData("*test*", "testing", true)]
        [InlineData("*test*", "stress testing", true)]
        [InlineData("*test*", "tes", false)]
        [InlineData("*test1*test2*", "test1 test2", true)]
        [InlineData("*test1*test2*", "test0 test1 test2 test3", true)]
        [InlineData("*test1*test2*", "test test2", false)]
        [InlineData("/path?q=*", "/path?q=1", true)]
        [InlineData("/path?q=*", "/path?v=1&q=1", false)]
        [InlineData("/path/file.jpg?q=*&p=1&m=[a,b]", "/path/file.jpg?q=search%20term&p=1&m=[a,b]", true)]
        [InlineData("/path/file.jpg?q=*&p=1&m=[a,b]", "/path/file.jpg?q=search%20term&p=1&m=(a,b)", false)]
        [InlineData("/path/file.jpg?q=*&p=*&m=*", "/path/file.jpg?q=search%20term&p=1&m=[a,b]", true)]
        public void Given_that_valueToMatch_matches_value_when_matching_it_should_pass(string value, string valueToMatch, bool shouldMatch)
        {
            var sut = Matches.Wildcard(value);
            sut.IsMatch(valueToMatch).Should().Be(shouldMatch);
        }
    }

    public sealed class RegexTests : MatchesTests
    {
        [Fact]
        public void Given_that_regex_string_is_null_when_creating_instance_it_should_throw()
        {
            string? regex = null;

            // Act
            Func<Matches> act = () => Matches.Regex(regex!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(regex));
        }

        [Fact]
        public void Given_that_regex_is_null_when_creating_instance_it_should_throw()
        {
            Regex? regex = null;

            // Act
            Func<Matches> act = () => Matches.Regex(regex!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(regex));
        }

        [Theory]
        [InlineData("test")]
        [InlineData("^t.+st.*")]
        public void When_creating_regex_matcher_it_should_return_expected_instance(string regex)
        {
            // Act
            var sut = Matches.Regex(regex);

            // Assert
            sut.Value.Should()
                .Be(sut.ToString())
                .And.Be(regex);
            sut.IsMatch.Should().NotBeNull();
        }

        [Theory]
        [InlineData("^123$", "123", true)]
        [InlineData("^123$", "1234", false)]
        [InlineData("^123", "1234", true)]
        public void Given_that_valueToMatch_matches_regex_when_matching_it_should_pass(
            string regex,
            string valueToMatch,
            bool shouldMatch
        )
        {
            var sut = Matches.Regex(regex);
            sut.IsMatch(valueToMatch).Should().Be(shouldMatch);
        }
    }

    public sealed class ExpressionTests : MatchesTests
    {
        [Fact]
        public void Given_that_expression_is_null_when_creating_instance_it_should_throw()
        {
            Expression<Func<string, bool>>? expression = null;

            // Act
            Func<Matches> act = () => Matches.Expression(expression!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(expression));
        }

        [Fact]
        public void When_creating_matcher_it_should_return_expected_instance()
        {
            Expression<Func<string, bool>> expression = s => s == "1";

            // Act
            var sut = Matches.Expression(expression);

            // Assert
            sut.Value.Should()
                .Be(sut.ToString())
                .And.Be(expression.ToString());
            sut.IsMatch.Should().NotBeNull();
        }

        [Theory]
        [InlineData("1", false)]
        [InlineData("12", true)]
        [InlineData("123", true)]
        [InlineData("1abc", true)]
        [InlineData("1c", false)]
        public void Given_that_valueToMatch_matches_expression_when_matching_it_should_pass(string valueToMatch, bool shouldMatch)
        {
            Expression<Func<string, bool>> expression = s => s.StartsWith("12") || s.EndsWith("bc");

            // Act
            var sut = Matches.Expression(expression);

            // Assert
            sut.IsMatch(valueToMatch).Should().Be(shouldMatch);
        }
    }

    public sealed class OperatorTests : MatchesTests
    {
        [Fact]
        public void Given_that_value_is_string_when_implicit_casting_it_should_return_exact_string_matcher()
        {
            const string value = "123abc";

            // Act
            Matches actual = value;

            // Assert
            actual.Value.Should().Be(value);
            actual.IsMatch(value).Should().BeTrue();
            actual.IsMatch(value + "d").Should().BeFalse();
        }

        [Fact]
        public void When_negating_it_should_not_match()
        {
            Matches matches = Matches.Any;

            // Act
            Matches actual = !matches;

            // Assert
            actual.Value.Should()
                .Be(actual.ToString())
                .And.Be("!= Any");
            actual.IsMatch("test").Should().BeFalse();
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("123", false)]
        [InlineData("abc", false)]
        [InlineData("123abc", true)]
        [InlineData("123abcDEF", true)]
        [InlineData("abcDEF", false)]
        public void When_and_ing_it_should_match_both(string valueToTest, bool shouldMatch)
        {
            var matcher1 = Matches.Regex(@"\d+");
            var matcher2 = Matches.Wildcard("*abc*");

            // Act
            Matches actual = matcher1 & matcher2;

            // Assert
            actual.Value.Should()
                .Be(actual.ToString())
                .And.Be(@"(\d+ & *abc*)");
            actual.IsMatch(valueToTest).Should().Be(shouldMatch);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("123", true)]
        [InlineData("abc", true)]
        [InlineData("123abc", true)]
        [InlineData("123abcDEF", true)]
        [InlineData("abcDEF", true)]
        public void When_or_ing_it_should_match_either_or_both(string valueToTest, bool shouldMatch)
        {
            var matcher1 = Matches.Regex(@"\d+");
            var matcher2 = Matches.Wildcard("*abc*");

            // Act
            Matches actual = matcher1 | matcher2;

            // Assert
            actual.Value.Should()
                .Be(actual.ToString())
                .And.Be(@"(\d+ | *abc*)");
            actual.IsMatch(valueToTest).Should().Be(shouldMatch);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("123", true)]
        [InlineData("abc", true)]
        [InlineData("123abc", false)]
        [InlineData("123abcDEF", false)]
        [InlineData("abcDEF", true)]
        public void When_xor_ing_it_should_match_either(string valueToTest, bool shouldMatch)
        {
            var matcher1 = Matches.Regex(@"\d+");
            var matcher2 = Matches.Wildcard("*abc*");

            // Act
            Matches actual = matcher1 ^ matcher2;

            // Assert
            actual.Value.Should()
                .Be(actual.ToString())
                .And.Be(@"(\d+ ^ *abc*)");
            actual.IsMatch(valueToTest).Should().Be(shouldMatch);
        }

        [Theory]
        [InlineData(
            "",
            false,
            0,
            1
        )]
        [InlineData("123", true, 0)]
        [InlineData(
            "abc",
            false,
            0,
            1,
            2
        )]
        [InlineData(
            "123abc",
            true,
            0
        )]
        [InlineData(
            "123abcDEF",
            true,
            0
        )]
        [InlineData(
            "abcDEF",
            true,
            0,
            1,
            2
        )]
        public void It_should_short_circuit(string valueToTest, bool shouldMatch, params int[] expectedEvaluatedMatchers)
        {
            Matches matcher1 = CreateMockedStringMatcher(@"\d+");
            Matches matcher2 = CreateMockedStringMatcher("[a-z]+");
            Matches matcher3 = CreateMockedStringMatcher("[A-Z]+");
            Matches[] allMatchers = [matcher1, matcher2, matcher3];

            // Act
            Matches actual = matcher1 | (matcher2 & matcher3);

            // Assert
            actual.Value.Should()
                .Be(actual.ToString())
                .And.Be(@"(\d+ | ([a-z]+ & [A-Z]+))");
            actual.IsMatch(valueToTest).Should().Be(shouldMatch);
            for (int i = 0; i < allMatchers.Length; i++)
            {
                Matches matches = allMatchers[i];
                if (expectedEvaluatedMatchers.Contains(i))
                {
                    matches.IsMatch.Received().Invoke(Arg.Any<string>());
                }
                else
                {
                    matches.IsMatch.DidNotReceive().Invoke(Arg.Any<string>());
                }
            }

            return;

            static Matches CreateMockedStringMatcher(string regex)
            {
                var matcher = new Matches
                {
                    Value = regex,
                    IsMatch = Substitute.For<Func<string, bool>>()
                };
                matcher.IsMatch
                    .Invoke(Arg.Any<string>())
                    .Returns(callInfo => Regex.IsMatch((string)callInfo[0], matcher.Value));
                return matcher;
            }
        }
    }
}
