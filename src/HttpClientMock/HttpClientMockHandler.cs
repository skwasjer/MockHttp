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
	public sealed class HttpClientMockHandler : HttpMessageHandler
	{
		private readonly List<MockedHttpRequest> _mockedRequests;
		private readonly MockedHttpRequest _fallback;

		public HttpClientMockHandler()
		{
			_mockedRequests = new List<MockedHttpRequest>();
			InvokedRequests = new InvokedHttpRequestCollection();

			_fallback = new MockedHttpRequest();
			Fallback = new HttpRequestSetupPhrase(_fallback);
			Fallback.RespondWithAsync(_ => CreateDefaultResponse());
		}

		public IInvokedHttpRequestCollection InvokedRequests { get; }

		public IRespondsThrows Fallback { get; }

		/// <inheritdoc />
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// Not thread safe...
			foreach (MockedHttpRequest mockedRequest in _mockedRequests)
			{
				if (mockedRequest.Matches(request))
				{
					return SendAsync(mockedRequest, request, cancellationToken);
				}
			}

			return SendAsync(_fallback, request, cancellationToken);
		}

		private Task<HttpResponseMessage> SendAsync(MockedHttpRequest mockedRequest, HttpRequestMessage request, CancellationToken cancellationToken)
		{
			((InvokedHttpRequestCollection)InvokedRequests).Add(new InvokedHttpRequest
			{
				MockedRequest = mockedRequest,
				Request = request
			});
			return mockedRequest.SendAsync(request, cancellationToken);
		}

		public IConfiguredRequest When(Action<RequestMatching> matching)
		{
			var b = new RequestMatching();
			matching(b);

			var newRequest = new MockedHttpRequest();
			newRequest.SetMatchers(b.Build());
			Add(newRequest);
			return new HttpRequestSetupPhrase(newRequest);
		}

		//public void Verify(IMockedHttpRequest request, string because = null)
		//{
		//	if (InvokedRequests.Cast<InvokedHttpRequest>().All(ir => request != ir.MockedRequest))
		//	{
		//		throw new InvalidOperationException($"Expected request to have been sent at least once{BecauseMessage(because)}, but it was not.{Environment.NewLine}   Expected = {request}");
		//	}
		//}

		//public void Verify(IMockedHttpRequest request, int count, string because = null)
		//{
		//	int actualCount = InvokedRequests.Cast<InvokedHttpRequest>().Count(ir => request == ir.MockedRequest);
		//	if (actualCount != count)
		//	{
		//		throw new InvalidOperationException($"Expected request to have been sent {count} time(s){BecauseMessage(because)}, but instead was sent {actualCount} time(s).{Environment.NewLine}   Expected = {request}");
		//	}
		//}

		public void Verify(Action<RequestMatching> matching, string because = null)
		{
			var rm = new RequestMatching();
			matching(rm);
			IReadOnlyCollection<IHttpRequestMatcher> shouldMatch = rm.Build();
			if (shouldMatch.Count == 0)
			{
				throw new ArgumentException($"At least one match needs to be configured.", nameof(matching));
			}

			if (!InvokedRequests.Any(r => shouldMatch.All(m => m.IsMatch(r.Request))))
			{
				throw new InvalidOperationException($"Expected request to have been sent at least once{BecauseMessage(because)}, but it was not.{Environment.NewLine}");
			}
		}

		public void Verify()
		{
			IEnumerable<MockedHttpRequest> verifiableMockedRequests = _mockedRequests
				.Where(r => !r.IsVerified && r.IsVerifiable);

			var expectedInvocations = new List<MockedHttpRequest>();
			foreach (MockedHttpRequest verifiableMockedRequest in verifiableMockedRequests)
			{
				var shouldMatch = verifiableMockedRequest.Matchers;
				if (shouldMatch.Count == 0)
				{
					continue;
				}

				if (!InvokedRequests.Any(r => shouldMatch.All(m => m.IsMatch(r.Request))))
				{
					expectedInvocations.Add(verifiableMockedRequest);
				}
				else
				{
					verifiableMockedRequest.IsVerified = true;
				}
			}

			if (expectedInvocations.Any())
			{
				throw new InvalidOperationException($"There are {expectedInvocations.Count} unfulfilled expectations:{Environment.NewLine}{string.Join(Environment.NewLine, expectedInvocations.Select(r => '\t' + r.ToString()))}");
			}
		}

		private void Add(MockedHttpRequest mockedRequest)
		{
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
