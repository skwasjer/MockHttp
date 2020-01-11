﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MockHttp.Responses
{
	public class ResponseFuncStrategyTests
	{
		[Fact]
		public void Given_null_response_when_creating_instance_should_throw()
		{
			Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFunc = null;

			// ReSharper disable once ObjectCreationAsStatement
			// ReSharper disable once ExpressionIsAlwaysNull
			Action act = () => new ResponseFuncStrategy(responseFunc);

			// Assert
			act.Should()
				.Throw<ArgumentNullException>()
				.Which.ParamName.Should()
				.Be(nameof(responseFunc));
		}

		[Fact]
		public void Given_response_factory_returns_null_should_not_throw()
		{
			// ReSharper disable once ConvertToLocalFunction
			Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFunc = (_, __) => null;

			var sut = new ResponseFuncStrategy(responseFunc);

			// Act
			HttpResponseMessage responseMessage = null;
			Func<Task> act = async () => responseMessage = await sut.ProduceResponseAsync(new MockHttpRequestContext(new HttpRequestMessage()), CancellationToken.None);

			// Assert
			act.Should().NotThrow();
			responseMessage.Should().BeNull();
		}

		[Fact]
		public async Task When_getting_response_should_call_func_with_args_and_return_response()
		{
			using var cts = new CancellationTokenSource();
			using var responseMessage = new HttpResponseMessage();
			using var requestMessage = new HttpRequestMessage();
			HttpRequestMessage arg1 = null;
			CancellationToken arg2 = CancellationToken.None;

			// ReSharper disable once ConvertToLocalFunction
			Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFunc = (r, t) =>
			{
				arg1 = r;
				arg2 = t;
				// ReSharper disable once AccessToDisposedClosure
				return Task.FromResult(responseMessage);
			};

			var sut = new ResponseFuncStrategy(responseFunc);

			// Act
			HttpResponseMessage actualResponseMessage = await sut.ProduceResponseAsync(new MockHttpRequestContext(requestMessage), cts.Token);

			// Assert
			actualResponseMessage.Should().BeSameAs(responseMessage);
			arg1.Should().BeSameAs(requestMessage);
			arg2.Should().Be(cts.Token).And.NotBe(CancellationToken.None);
		}
	}
}
