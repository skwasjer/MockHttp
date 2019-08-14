using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MockHttp.Language;
using MockHttp.Language.Flow;
using MockHttp.Matchers;

namespace MockHttp
{
	/// <summary>
	/// Represents a message handler that can be used to mock HTTP responses and verify HTTP requests sent via <see cref="HttpClient"/>.
	/// </summary>
	public sealed class MockHttpHandler : HttpMessageHandler
	{
		private readonly List<HttpCall> _setups;
		private readonly HttpCall _fallbackSetup;

		/// <summary>
		/// Initializes a new instance of the <see cref="MockHttpHandler"/> class.
		/// </summary>
		public MockHttpHandler()
		{
			_setups = new List<HttpCall>();
			InvokedRequests = new InvokedHttpRequestCollection();

			_fallbackSetup = new HttpCall();
			Fallback = new HttpRequestSetupPhrase(_fallbackSetup);
			Reset();
		}

		/// <summary>
		/// Gets a collection of invoked requests that were handled.
		/// </summary>
		public IInvokedHttpRequestCollection InvokedRequests { get; }

		/// <summary>
		/// Gets a fallback configurer that can be used to configure the default response if no expectation was matched.
		/// </summary>
		public IRespondsThrows Fallback { get; }

		/// <inheritdoc />
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			await LoadIntoBufferAsync(request.Content).ConfigureAwait(false);

			// Not thread safe...
			foreach (HttpCall setup in _setups)
			{
				if (setup.Matchers.All(request, out IEnumerable<HttpRequestMatcher> notMatchedOn))
				{
					return await SendAsync(setup, request, cancellationToken).ConfigureAwait(false);
				}
			}

			return await SendAsync(_fallbackSetup, request, cancellationToken).ConfigureAwait(false);
		}

		private Task<HttpResponseMessage> SendAsync(HttpCall setup, HttpRequestMessage request, CancellationToken cancellationToken)
		{
			((InvokedHttpRequestCollection)InvokedRequests).Add(new InvokedHttpRequest(setup, request));
			return setup.SendAsync(request, cancellationToken);
		}

		/// <summary>
		/// Configures a condition for an expectation. If the condition evaluates to <see langword="false"/>, the expectation is not matched.
		/// </summary>
		/// <param name="matching">The request match builder.</param>
		/// <returns>The configured request.</returns>
		public IConfiguredRequest When(Action<RequestMatching> matching)
		{
			if (matching == null)
			{
				throw new ArgumentNullException(nameof(matching));
			}

			var b = new RequestMatching();
			matching(b);

			var newSetup = new HttpCall();
			newSetup.SetMatchers(b.Build());
			_setups.Add(newSetup);
			return new HttpRequestSetupPhrase(newSetup);
		}

		/// <summary>
		/// Resets this mock's state. This includes its setups, configured default return values, and all recorded invocations.
		/// </summary>
		public void Reset()
		{
			InvokedRequests.Clear();
			_fallbackSetup.Reset();
			_setups.Clear();

			Fallback.Respond(_ => CreateDefaultResponse());
		}

		/// <summary>
		/// Verifies that a request matching the specified match conditions has been sent.
		/// </summary>
		/// <param name="matching">The conditions to match.</param>
		/// <param name="times">The number of times a request is allowed to be sent.</param>
		/// <param name="because">The reasoning for this expectation.</param>
		public void Verify(Action<RequestMatching> matching, Func<IsSent> times, string because = null)
		{
			if (times == null)
			{
				throw new ArgumentNullException(nameof(times));
			}

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
			if (matching == null)
			{
				throw new ArgumentNullException(nameof(matching));
			}

			var rm = new RequestMatching();
			matching(rm);
			IReadOnlyCollection<HttpRequestMatcher> shouldMatch = rm.Build();

			IReadOnlyList<IInvokedHttpRequest> matchedRequests = shouldMatch.Count == 0
				? (IReadOnlyList<IInvokedHttpRequest>)InvokedRequests
				: InvokedRequests.Where(r => shouldMatch.All(r.Request, out _)).ToList();

			if (!times.Verify(matchedRequests.Count))
			{
				throw new HttpMockException(times.GetErrorMessage(matchedRequests.Count, BecauseMessage(because)));
			}

			foreach (InvokedHttpRequest r in matchedRequests.Cast<InvokedHttpRequest>())
			{
				r.MarkAsVerified();
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
		[Obsolete("Renamed to " + nameof(VerifyNoOtherRequests) + ". This method will be removed in future.")]
		public void VerifyNoOtherCalls()
		{
			VerifyNoOtherRequests();
		}

		/// <summary>
		/// Verifies that there were no requests sent other than those already verified.
		/// </summary>
		public void VerifyNoOtherRequests()
		{
			List<InvokedHttpRequest> unverifiedRequests = InvokedRequests
				.Cast<InvokedHttpRequest>()
				.Where(r => !r.IsVerified)
				.ToList();
			if (!unverifiedRequests.Any())
			{
				return;
			}

			string unverifiedRequestsStr = string.Join(Environment.NewLine, unverifiedRequests.Select(ir => ir.Request.ToString()));

			throw new HttpMockException($"There are {unverifiedRequests.Count} unverified requests:{Environment.NewLine}{unverifiedRequestsStr}");
		}

		private void Verify(IEnumerable<HttpCall> verifiableSetups)
		{
			List<HttpCall> expectedInvocations = verifiableSetups
				.Where(setup => !setup.VerifyIfInvoked())
				.ToList();
			if (!expectedInvocations.Any())
			{
				return;
			}

			string invokedRequestsStr = string.Join(Environment.NewLine, InvokedRequests.Select(ir => ir.Request.ToString()));
			if (invokedRequestsStr.Length > 0)
			{
				invokedRequestsStr = Environment.NewLine + "Seen requests: " + invokedRequestsStr;
			}

			throw new HttpMockException($"There are {expectedInvocations.Count} unfulfilled expectations:{Environment.NewLine}{string.Join(Environment.NewLine, expectedInvocations.Select(r => '\t' + r.ToString()))}{invokedRequestsStr}");
		}

		private static HttpResponseMessage CreateDefaultResponse()
		{
			return new HttpResponseMessage(HttpStatusCode.NotFound)
			{
				ReasonPhrase = "No request is configured, returning default response."
			};
		}

		private static async Task LoadIntoBufferAsync(HttpContent httpContent)
		{
			if (httpContent != null)
			{
				// Force read content, so content can be checked more than once.
				await httpContent.LoadIntoBufferAsync().ConfigureAwait(false);
				// Force read content length, in case it will be checked via header matcher.
				// ReSharper disable once UnusedVariable
				long? cl = httpContent.Headers.ContentLength;
			}
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
