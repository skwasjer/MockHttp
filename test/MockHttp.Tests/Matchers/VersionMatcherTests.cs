using System;
using System.Net.Http;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Matchers
{
	public class VersionMatcherTests
	{
		private VersionMatcher _sut;

		[Fact]
		public void Given_version_matches_expected_version_when_matching_should_match()
		{
			var request = new HttpRequestMessage
			{
				Version = new Version(2, 0)
			};

			_sut = new VersionMatcher(new Version(2, 0));

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Fact]
		public void Given_version_does_not_matches_expected_version_when_matching_should_not_match()
		{
			var request = new HttpRequestMessage
			{
				Version = new Version(2, 0)
			};

			_sut = new VersionMatcher(new Version(2, 1));

			// Act & assert
			_sut.IsMatch(request).Should().BeFalse();
		}

		[Fact]
		public void Given_version_is_null_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new VersionMatcher(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("version");
		}
	}
}
