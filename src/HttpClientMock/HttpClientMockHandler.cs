using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMock
{
	public class HttpClientMockHandler : HttpMessageHandler, IMockHttpRequestBuilder
	{
		private readonly List<IMockedHttpRequest> _mockedRequests;
		private readonly InvokedRequestList _invokedRequests;

		public HttpClientMockHandler()
		{
			_mockedRequests = new List<IMockedHttpRequest>();
			_invokedRequests = new InvokedRequestList();
			//Expect = new ExpectedRequests(this);

			Fallback = new MockedHttpRequest(this);
			((MockedHttpRequest)Fallback).RespondsWith(() => Task.FromResult(CreateDefaultResponse()));
		}

		public IInvokedRequestList InvokedRequests => _invokedRequests;

		public IMockedHttpRequest Fallback { get; set; }

		/// <inheritdoc />
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			//IMockedHttpRequest nextExpectation = Expect.Requests.Peek();
			//if (nextExpectation != null)
			//{
			//	if (nextExpectation.Matches(request))
			//	{
			//		try
			//		{
			//			return SendAsync(nextExpectation, request, cancellationToken);
			//		}
			//		finally
			//		{
			//			Expect.Requests.Dequeue();
			//		}
			//	}
			//}
			//else
			{
				foreach (IMockedHttpRequest mockedRequest in _mockedRequests)
				{
					if (mockedRequest.Matches(request))
					{
						return SendAsync(mockedRequest, request, cancellationToken);
					}
				}
			}

			return SendAsync(Fallback, request, cancellationToken);
		}

		private Task<HttpResponseMessage> SendAsync(IMockedHttpRequest mockedRequest, HttpRequestMessage request, CancellationToken cancellationToken)
		{
			_invokedRequests.Add(mockedRequest);
			return mockedRequest.SendAsync(request, cancellationToken);
		}

		//public ExpectedRequests Expect { get; }

		public IMockedHttpRequest WhenRequesting(IMockedHttpRequest mockedRequest)
		{
//			var builder = new HttpRequestMockBuilder(this);
			_mockedRequests.Add(mockedRequest);
			return mockedRequest;
		}

		public void Verify(IMockedHttpRequest request, string because = null)
		{
			if (_invokedRequests.All(httpRequest => request != httpRequest))
			{
				throw new InvalidOperationException($"Expected request to have been sent at least once{BecauseMessage(because)}, but it was not.{Environment.NewLine}   Expected = {request}");
			}
		}

		public void Verify(IMockedHttpRequest request, int count, string because = null)
		{
			int actualCount = _invokedRequests.Count(httpRequest => request == httpRequest);
			if (actualCount != count)
			{
				throw new InvalidOperationException($"Expected request to have been sent {count} time(s){BecauseMessage(because)}, but instead was sent {actualCount} time(s).{Environment.NewLine}   Expected = {request}");
			}
		}

		//public void Verify()
		//{
		//	if (Expect.Requests.Any())
		//	{
		//		throw new InvalidOperationException($"There are {Expect.Requests.Count} unfulfilled expectations:{Environment.NewLine}{string.Join(Environment.NewLine, Expect.Requests.Select(r => '\t' + r.ToString()))}");
		//	}
		//}

		private static HttpResponseMessage CreateDefaultResponse()
		{
			return new HttpResponseMessage(HttpStatusCode.NotFound)
			{
				ReasonPhrase = "No request/response mocked."
			};
		}

		private static string BecauseMessage(string because)
		{
			if (string.IsNullOrWhiteSpace(because))
			{
				return string.Empty;
			}

			because = because.TrimStart(' ');
			return because.StartsWith("because", StringComparison.OrdinalIgnoreCase)
				? " " + because
				: " because " + because;
		}
	}
}
