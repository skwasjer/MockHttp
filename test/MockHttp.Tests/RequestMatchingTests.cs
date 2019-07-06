using System;
using System.Collections.Generic;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Matchers;
using Xunit;

namespace MockHttp
{
	public class RequestMatchingTests
	{
		private readonly RequestMatching _sut;

		public RequestMatchingTests()
		{
			_sut = new RequestMatching();
		}

		[Fact]
		public void Given_two_instances_of_same_type_are_added_when_building_should_return_both_instances()
		{
			var matcher1 = new ContentMatcher();
			var matcher2 = new ContentMatcher();
			_sut.With(matcher1)
				.With(matcher2);

			// Act
			IReadOnlyCollection<IHttpRequestMatcher> actual = _sut.Build();

			// Assert
			actual.Should().BeEquivalentTo(matcher1, matcher2);
		}

		[Fact]
		public void Given_same_instance_is_added_more_than_once_when_building_should_return_only_return_one()
		{
			var matcher = new ContentMatcher();
			_sut.With(matcher)
				.With(matcher);

			// Act
			IReadOnlyCollection<IHttpRequestMatcher> actual = _sut.Build();

			// Assert
			actual.Should().BeEquivalentTo(matcher);
		}

		[Fact]
		public void Given_existing_instance_is_replaced_when_building_should_return_last()
		{
			var matcher1 = new ContentMatcher();
			var matcher2 = new ContentMatcher();
			_sut.With(matcher1)
				.Replace(matcher2);

			// Act
			IReadOnlyCollection<IHttpRequestMatcher> actual = _sut.Build();

			// Assert
			actual.Should().BeEquivalentTo(matcher2);
		}

		[Fact]
		public void Given_nothing_to_replace_when_building_should_not_throw_but_add()
		{
			var matcher = new ContentMatcher();
			_sut.Replace(matcher);

			// Act
			IReadOnlyCollection<IHttpRequestMatcher> actual = _sut.Build();

			// Assert
			actual.Should().BeEquivalentTo(matcher);
		}

		[Fact]
		public void When_adding_null_instance_should_throw()
		{
			Action act = () => _sut.With(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("matcher");
		}

		[Fact]
		public void When_replacing_null_instance_should_throw()
		{
			Action act = () => _sut.Replace(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("matcher");
		}
	}
}