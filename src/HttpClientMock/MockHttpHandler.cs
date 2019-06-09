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
			Reset();
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
			((InvokedHttpRequestCollection)InvokedRequests).Add(new InvokedHttpRequest(setup, request));
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

		/// <summary>
		/// Resets this mock's state. This includes its setups, configured default return values, and all recorded invocations.
		/// </summary>
		public void Reset()
		{
			InvokedRequests.Reset();
			_fallbackSetup.Reset();
			_setups.Clear();

			Fallback.RespondWithAsync(_ => CreateDefaultResponse());
		}

		/// <summary>
		/// Verifies that a request matching the specified match conditions has been sent.
		/// </summary>
		/// <param name="matching">The conditions to match.</param>
		/// <param name="times">The number of times a request is allowed to be sent.</param>
		/// <param name="because">The reasoning for this expectation.</param>
		public void Verify(Action<RequestMatching> matching, Func<IsSent> times, string because = null)
		{
			Verify(matching, times(), because);
		}

		/// <summary>
		/// Verifies that a request matching the specified match conditions has been sent.
		/// </summary>
		/// <param name="matching">The conditions to match.</param>
		/// <param name="times">The number of times a request is allowed to be sent.</param>
		/// <param name="because">The reasoning for this expectation.</param>
		public void Verify(Action<RequestMatching> matching, IsSent times, string because = null)
		{
			var rm = new RequestMatching();
			matching(rm);
			IReadOnlyCollection<IHttpRequestMatcher> shouldMatch = rm.Build();

			int callCount = shouldMatch.Count == 0
				? InvokedRequests.Count
				: InvokedRequests.Count(r => shouldMatch.AreAllMatching(r.Request));

			if (!times.Verify(callCount))
			{
				throw new HttpMockException(times.GetErrorMessage(callCount, BecauseMessage(because)));
			}
		}

		/// <summary>
		/// Verifies that all verifiable expected requests have been sent.
		/// </summary>
		public void Verify()
		{
			IEnumerable<HttpCall> verifiableSetups = _setups.Where(r => r.IsVerifiable);

			Verify(verifiableSetups);
		}

		/// <summary>
		/// Verifies all expected requests regardless of whether they have been flagged as verifiable.
		/// </summary>
		public void VerifyAll()
		{
			Verify(_setups);
		}

		/// <summary>
		/// Verifies that there were no requests sent other than those already verified.
		/// </summary>
		public void VerifyNoOtherCalls()
		{
			IEnumerable<HttpCall> unverifiedSetups = _setups.Where(r => !r.IsVerified);
			Verify(unverifiedSetups);
		}

		private static void Verify(IEnumerable<HttpCall> verifiableSetups)
		{
			var expectedInvocations = new List<HttpCall>();
			foreach (HttpCall setup in verifiableSetups)
			{
				if (setup.IsInvoked)
				{
					setup.IsVerified = true;
				}
				else
				{
					expectedInvocations.Add(setup);
				}
			}

			if (expectedInvocations.Any())
			{
				throw new HttpMockException($"There are {expectedInvocations.Count} unfulfilled expectations:{Environment.NewLine}{string.Join(Environment.NewLine, expectedInvocations.Select(r => '\t' + r.ToString()))}");
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
