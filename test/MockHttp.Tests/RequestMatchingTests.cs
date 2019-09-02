using System;
using System.Collections.Generic;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Matchers;
using Moq;
using Xunit;

namespace MockHttp
{
	public class RequestMatchingTests
	{
		private readonly RequestMatching _sut;
		private readonly HttpRequestMatcher _matcher1;
		private readonly HttpRequestMatcher _matcher2;
		private bool _isExclusive1;
		private bool _isExclusive2;

		public RequestMatchingTests()
		{
			HttpRequestMatcher CreateMatcherStub(Func<bool> returns)
			{
				var matcherMock = new Mock<HttpRequestMatcher>();
				matcherMock
					.Setup(m => m.IsExclusive)
					.Returns(returns);
				return matcherMock.Object;
			}

			_sut = new RequestMatching();

			_matcher1 = CreateMatcherStub(() => _isExclusive1);
			_matcher2 = CreateMatcherStub(() => _isExclusive2);
		}

		[Fact]
		public void Given_two_non_mutually_exclusive_instances_of_same_type_when_building_should_return_both_instances()
		{
			_isExclusive1 = false;
			_isExclusive2 = false;

			_sut.With(_matcher1)
				.With(_matcher2);

			// Act
			IReadOnlyCollection<IAsyncHttpRequestMatcher> actual = _sut.Build();

			// Assert
			actual.Should().BeEquivalentTo(_matcher1, _matcher2);
		}

		[Fact]
		public void Given_two_mutually_exclusive_instances_of_same_type_when_adding_should_throw()
		{
			_isExclusive1 = true;
			_isExclusive2 = true;
			_sut.With(_matcher1);

			// Act
			Action act = () => _sut.With(_matcher2);

			// Assert
			act.Should().Throw<InvalidOperationException>().WithMessage("Cannot add matcher*");
		}

		[Fact]
		public void Given_two_instances_of_same_type_of_which_first_is_exclusive_when_adding_second_should_throw()
		{
			_isExclusive1 = true;
			_isExclusive2 = false;
			_sut.With(_matcher1);

			// Act
			Action act = () => _sut.With(_matcher2);

			// Assert
			act.Should().Throw<InvalidOperationException>().WithMessage("Cannot add matcher*");
		}

		[Fact]
		public void Given_two_instances_of_same_type_of_which_second_is_exclusive_when_adding_second_should_throw()
		{
			_isExclusive1 = false;
			_isExclusive2 = true;
			_sut.With(_matcher1);

			// Act
			Action act = () => _sut.With(_matcher2);

			// Assert
			act.Should().Throw<InvalidOperationException>().WithMessage("Cannot add matcher*");
		}

		[Fact]
		public void Given_same_instance_is_added_more_than_once_when_building_should_return_only_return_one()
		{
			_sut.With(_matcher1)
				.With(_matcher1);

			// Act
			IReadOnlyCollection<IAsyncHttpRequestMatcher> actual = _sut.Build();

			// Assert
			actual.Should().BeEquivalentTo(_matcher1);
		}

		[Fact]
		public void When_adding_null_instance_should_throw()
		{
			Action act = () => _sut.With(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("matcher");
		}
	}
}