using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpClientMock.Language;
using HttpClientMock.Language.Flow;

namespace HttpClientMock
{
	public sealed class MockHttpHandler : HttpMessageHandler
	{
		private readonly List<HttpCall> _setups;
		private readonly HttpCall _fallbackSetup;

		public MockHttpHandler()
		{
			_setups = new List<HttpCall>();
			InvokedRequests = new InvokedHttpRequestCollection();

			_fallbackSetup = new HttpCall();
			Fallback = new HttpRequestSetupPhrase(_fallbackSetup);
			Fallback.RespondWithAsync(_ => CreateDefaultResponse());
		}

		public IInvokedHttpRequestCollection InvokedRequests { get; }

		public IRespondsThrows Fallback { get; }

		/// <inheritdoc />
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// Not thread safe...
			foreach (HttpCall setup in _setups)
			{
				if (setup.Matches(request))
				{
					return SendAsync(setup, request, cancellationToken);
				}
			}

			return SendAsync(_fallbackSetup, request, cancellationToken);
		}

		private Task<HttpResponseMessage> SendAsync(HttpCall setup, HttpRequestMessage request, CancellationToken cancellationToken)
		{
			((InvokedHttpRequestCollection)InvokedRequests).Add(new InvokedHttpRequest
			{
				Setup = setup,
				Request = request
			});
			return setup.SendAsync(request, cancellationToken);
		}

		public IConfiguredRequest When(Action<RequestMatching> matching)
		{
			var b = new RequestMatching();
			matching(b);

			var newSetup = new HttpCall();
			newSetup.SetMatchers(b.Build());
			Add(newSetup);
			return new HttpRequestSetupPhrase(newSetup);
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
			IEnumerable<HttpCall> verifiableSetups = _setups
				.Where(r => !r.IsVerified && r.IsVerifiable);

			var expectedInvocations = new List<HttpCall>();
			foreach (HttpCall setup in verifiableSetups)
			{
				if (setup.Matchers.Count == 0)
				{
					continue;
				}

				if (!InvokedRequests.Any(r => setup.Matchers.All(m => m.IsMatch(r.Request))))
				{
					expectedInvocations.Add(setup);
				}
				else
				{
					setup.IsVerified = true;
				}
			}

			if (expectedInvocations.Any())
			{
				throw new InvalidOperationException($"There are {expectedInvocations.Count} unfulfilled expectations:{Environment.NewLine}{string.Join(Environment.NewLine, expectedInvocations.Select(r => '\t' + r.ToString()))}");
			}
		}

		private void Add(HttpCall setup)
		{
			if (!_setups.Contains(setup))
			{
				_setups.Add(setup);
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
