using static MockHttp.Http.UriExtensions;

namespace MockHttp.Http;

public abstract class UriExtensionsTests
{
    public class EnsureIsRootedTests : UriExtensionsTests
    {
        [Theory]
        [InlineData("relative/path", "/relative/path")]
        [InlineData("/relative/path", "/relative/path")]
        [InlineData("http://0.0.0.0/relative/path", "http://0.0.0.0/relative/path")]
        public void Given_that_uri_does_not_start_with_forward_slash_when_ensuringIsRooted_it_should_modify_it(string uriStr, string expectedUriStr)
        {
            var uri = new Uri(uriStr, DotNetRelativeOrAbsolute);
            var expectedUri = new Uri(expectedUriStr, DotNetRelativeOrAbsolute);

            // Act
            Uri actual = uri.EnsureIsRooted();

            // Assert
            actual.Should().Be(expectedUri);
            if (actual == uri)
            {
                actual.Should().BeSameAs(uri);
            }
        }

        [Fact]
        public void Given_that_uri_is_null_when_ensuringIsRooted_it_should_throw()
        {
            Uri? uri = null;

            // Act
            Func<Uri> act = () => uri!.EnsureIsRooted();

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be(nameof(uri));
        }
    }
}
