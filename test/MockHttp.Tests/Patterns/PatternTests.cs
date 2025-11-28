using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace MockHttp.Patterns;

public abstract class PatternTests
{
    public sealed class InstanceTests
    {
        [Fact]
        public void Given_that_value_is_null_when_initializing_it_should_throw()
        {
            string? value = null;

            // Act
            Func<Pattern> act = () => new Pattern
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
            Func<Pattern> act = () => new Pattern
            {
                Value = string.Empty,
                IsMatch = value!
            };

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(value));
        }
    }

    public sealed class EmptyTests : PatternTests
    {
        [Fact]
        public void When_uninitialized_it_should_be_empty()
        {
#pragma warning disable S1481
#pragma warning disable CS8887
            Pattern sut;

            sut.Should().BeEquivalentTo(Pattern.Empty);
#pragma warning restore CS8887
#pragma warning restore S1481
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("123", false)]
        public void It_should_only_match_null_or_empty_strings(string? value, bool shouldMatch)
        {
            Pattern.Empty.IsMatch(value!).Should().Be(shouldMatch);
        }
    }

    public sealed class AnyTests : PatternTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData("abcdef")]
        public void It_should_always_match(string value)
        {
            Pattern.Any.IsMatch(value).Should().BeTrue();
        }

        [Fact]
        public void It_should_have_expected_value()
        {
            Pattern.Any.Value.Should()
                .Be(Pattern.Any.ToString())
                .And.Be(nameof(Pattern.Any));
            Pattern.Any.Should().BeEquivalentTo(Pattern.Any);
        }
    }

    public sealed class ExactTests : PatternTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData("abcdef")]
        public void It_should_match_exactly(string value)
        {
            Pattern.Exactly(value).IsMatch(value).Should().BeTrue();
        }

        [Theory]
        [InlineData("", " ")]
        [InlineData("123", "124")]
        [InlineData("abcdef", "Abcdef")]
        public void It_should_not_match_exactly(string pattern, string value)
        {
            Pattern.Exactly(pattern).IsMatch(value).Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData("abcdef")]
        public void It_should_have_expected_value(string value)
        {
            // Act
            var actual = Pattern.Exactly(value);

            // Assert
            actual.Value.Should()
                .Be(actual.ToString())
                .And.Be(value);
        }
    }

    public sealed class WildcardTests : PatternTests
    {
        [Fact]
        public void Given_that_pattern_is_null_when_creating_instance_it_should_throw()
        {
            string? pattern = null;

            // Act
            Func<Pattern> act = () => Pattern.Wildcard(pattern!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(pattern));
        }

        [Fact]
        public void Given_that_pattern_is_empty_when_creating_instance_it_should_throw()
        {
            string pattern = string.Empty;

            // Act
            Func<Pattern> act = () => Pattern.Wildcard(pattern);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName(nameof(pattern))
                .WithMessage("The pattern cannot be empty.*");
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
        public void When_creating_pattern_it_should_return_expected_instance(string wildcardPattern, string expectedRegex)
        {
            // Act
            var sut = WildcardPattern.Create(wildcardPattern);

            // Assert
            sut.RegexPattern.Value.Should().Be(expectedRegex);
            sut.Value.Should()
                .Be(sut.ToString())
                .And.Be(wildcardPattern);
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
        public void Given_that_value_matches_pattern_when_matching_it_should_pass(string wildcardPattern, string value, bool shouldMatch)
        {
            var sut = Pattern.Wildcard(wildcardPattern);
            sut.IsMatch(value).Should().Be(shouldMatch);
        }
    }

    public sealed class RegexTests : PatternTests
    {
        [Fact]
        public void Given_that_pattern_is_null_when_creating_instance_it_should_throw()
        {
            string? pattern = null;

            // Act
            Func<Pattern> act = () => Pattern.Regex(pattern!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(pattern));
        }

        [Fact]
        public void Given_that_regex_is_null_when_creating_instance_it_should_throw()
        {
            Regex? pattern = null;

            // Act
            Func<Pattern> act = () => Pattern.Regex(pattern!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(pattern));
        }

        [Theory]
        [InlineData("test")]
        [InlineData("^t.+st.*")]
        public void When_creating_pattern_it_should_return_expected_instance(string regexPattern)
        {
            // Act
            var sut = Pattern.Regex(regexPattern);

            // Assert
            sut.Value.Should()
                .Be(sut.ToString())
                .And.Be(regexPattern);
            sut.IsMatch.Should().NotBeNull();
        }

        [Theory]
        [InlineData("^123$", "123", true)]
        [InlineData("^123$", "1234", false)]
        [InlineData("^123", "1234", true)]
        public void Given_that_value_matches_pattern_when_matching_it_should_pass(string regexPattern, string value, bool shouldMatch)
        {
            var sut = Pattern.Regex(regexPattern);
            sut.IsMatch(value).Should().Be(shouldMatch);
        }
    }

    public sealed class ExpressionTests : PatternTests
    {
        [Fact]
        public void Given_that_expression_is_null_when_creating_instance_it_should_throw()
        {
            Expression<Func<string, bool>>? expression = null;

            // Act
            Func<Pattern> act = () => Pattern.Expression(expression!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName(nameof(expression));
        }

        [Fact]
        public void When_creating_pattern_it_should_return_expected_instance()
        {
            Expression<Func<string, bool>> expression = s => s == "1";

            // Act
            var sut = Pattern.Expression(expression);

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
        public void Given_that_value_matches_pattern_when_matching_it_should_pass(string value, bool shouldMatch)
        {
            Expression<Func<string, bool>> expression = s => s.StartsWith("12") || s.EndsWith("bc");

            // Act
            var sut = Pattern.Expression(expression);

            // Assert
            sut.IsMatch(value).Should().Be(shouldMatch);
        }
    }

    public sealed class OperatorTests : PatternTests
    {
        [Fact]
        public void Given_that_value_is_string_when_implicit_casting_it_should_return_exact_pattern_match()
        {
            const string value = "123abc";

            // Act
            Pattern actual = value;

            // Assert
            actual.Value.Should().Be(value);
            actual.IsMatch(value).Should().BeTrue();
            actual.IsMatch(value + "d").Should().BeFalse();
        }

        [Fact]
        public void When_negating_it_should_not_match()
        {
            Pattern pattern = Pattern.Any;

            // Act
            Pattern actual = !pattern;

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
            var pattern1 = Pattern.Regex(@"\d+");
            var pattern2 = Pattern.Wildcard("*abc*");

            // Act
            Pattern actual = pattern1 & pattern2;

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
            var pattern1 = Pattern.Regex(@"\d+");
            var pattern2 = Pattern.Wildcard("*abc*");

            // Act
            Pattern actual = pattern1 | pattern2;

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
            var pattern1 = Pattern.Regex(@"\d+");
            var pattern2 = Pattern.Wildcard("*abc*");

            // Act
            Pattern actual = pattern1 ^ pattern2;

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
        public void It_should_short_circuit(string valueToTest, bool shouldMatch, params int[] expectedEvaluatedPatterns)
        {
            Pattern pattern1 = CreateMockedPattern(@"\d+");
            Pattern pattern2 = CreateMockedPattern("[a-z]+");
            Pattern pattern3 = CreateMockedPattern("[A-Z]+");
            Pattern[] allPatterns = [pattern1, pattern2, pattern3];

            // Act
            Pattern actual = pattern1 | (pattern2 & pattern3);

            // Assert
            actual.Value.Should()
                .Be(actual.ToString())
                .And.Be(@"(\d+ | ([a-z]+ & [A-Z]+))");
            actual.IsMatch(valueToTest).Should().Be(shouldMatch);
            for (int i = 0; i < allPatterns.Length; i++)
            {
                Pattern pattern = allPatterns[i];
                if (expectedEvaluatedPatterns.Contains(i))
                {
                    pattern.IsMatch.Received().Invoke(Arg.Any<string>());
                }
                else
                {
                    pattern.IsMatch.DidNotReceive().Invoke(Arg.Any<string>());
                }
            }

            return;

            Pattern CreateMockedPattern(string regex)
            {
                var pattern = new Pattern
                {
                    Value = regex,
                    IsMatch = Substitute.For<Func<string, bool>>()
                };
                pattern.IsMatch
                    .Invoke(Arg.Any<string>())
                    .Returns(callInfo => Regex.IsMatch((string)callInfo[0], pattern.Value));
                return pattern;
            }
        }
    }
}
