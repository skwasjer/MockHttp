using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpClientMock.Language;

namespace HttpClientMock
{
	public class HttpClientMockHandler : HttpMessageHandler
	{
		private readonly List<IMockedHttpRequest> _mockedRequests;
		private readonly InvokedRequestList _invokedRequests;
		private readonly List<HttpRequestMessage> _invokedRequestMessages;
		private readonly Queue<IMockedHttpRequest> _expectations;
		private IMockedHttpRequest _fallback;

		public HttpClientMockHandler()
		{
			_mockedRequests = new List<IMockedHttpRequest>();
			_invokedRequests = new InvokedRequestList();
			_invokedRequestMessages = new List<HttpRequestMessage>();
			_expectations = new Queue<IMockedHttpRequest>();

			SetFallback(_ => CreateDefaultResponse());
		}

		public IInvokedRequestList InvokedRequests => _invokedRequests;

		public void SetFallback(Func<HttpRequestMessage, HttpResponseMessage> response)
		{
			_fallback = new MockedHttpRequest(
				new List<IHttpRequestMatcher>(),
				r => Task.FromResult(response(r)),
				null);
		}

		/// <inheritdoc />
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// Not thread safe...
			IMockedHttpRequest nextExpectation = _expectations.Count > 0
				? _expectations.Peek()
				: null;
			if (nextExpectation != null)
			{
				if (nextExpectation.Matches(request))
				{
					try
					{
						return SendAsync(nextExpectation, request, cancellationToken);
					}
					finally
					{
						_expectations.Dequeue();
					}
				}
			}
			else
			{
				foreach (IMockedHttpRequest mockedRequest in _mockedRequests)
				{
					if (mockedRequest.Matches(request))
					{
						return SendAsync(mockedRequest, request, cancellationToken);
					}
				}
			}

			return SendAsync(_fallback, request, cancellationToken);
		}

		private Task<HttpResponseMessage> SendAsync(IMockedHttpRequest mockedRequest, HttpRequestMessage request, CancellationToken cancellationToken)
		{
			_invokedRequests.Add(mockedRequest);
			_invokedRequestMessages.Add(request);
			return mockedRequest.SendAsync(request, cancellationToken);
		}

		public IConfiguredRequest When(Action<RequestMatching> matching)
		{
			return new MockedHttpRequestBuilder(this).When(matching);
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

		public void Verify(Action<RequestMatching> matching, string because = null)
		{
			var rm = new RequestMatching();
			matching(rm);
			IReadOnlyCollection<IHttpRequestMatcher> shouldMatch = rm.Build();
			if (shouldMatch.Count == 0)
			{
				throw new ArgumentException($"At least one match needs to be configured.", nameof(matching));
			}

			if (!_invokedRequestMessages.Any(r => shouldMatch.All(m => m.IsMatch(r))))
			{
				throw new InvalidOperationException($"Expected request to have been sent at least once{BecauseMessage(because)}, but it was not.{Environment.NewLine}");
			}
		}

		public void Verify()
		{
			if (_expectations.Any())
			{
				throw new InvalidOperationException($"There are {_expectations.Count} unfulfilled expectations:{Environment.NewLine}{string.Join(Environment.NewLine, _expectations.Select(r => '\t' + r.ToString()))}");
			}
		}

		internal void Add(IMockedHttpRequest mockedRequest)
		{
			if (mockedRequest is MockedHttpRequestBuilder builder && builder.IsVerifiable)
			{
				_expectations.Enqueue(mockedRequest);
				return;
			}

			if (!_mockedRequests.Contains(mockedRequest))
			{
				_mockedRequests.Add(mockedRequest);
			}
		}

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
