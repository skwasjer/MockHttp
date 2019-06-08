using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientMock
{
	internal sealed class MockedHttpRequest
	{
		private Func<HttpRequestMessage, Task<HttpResponseMessage>> _response;
		private Action<HttpRequestMessage> _callback;
		private string _verifiableBecause;
		private IReadOnlyCollection<IHttpRequestMatcher> _matchers;

		public IReadOnlyCollection<IHttpRequestMatcher> Matchers
		{
			get
			{
				if (_matchers == null)
				{
					SetMatchers(new List<IHttpRequestMatcher>());
				}

				return _matchers;
			}
		}

		internal bool IsVerifiable { get; set; }

		internal bool IsVerified { get; set; }

		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (_response == null)
			{
				// TODO: clarify which mock.
				throw new InvalidOperationException("No response configured for mock.");
			}

			cancellationToken.ThrowIfCancellationRequested();

			_callback?.Invoke(request);
			return _response(request);
		}

		public bool Matches(HttpRequestMessage request)
		{
			return Matchers.All(m => m.IsMatch(request));
		}

		internal void SetResponse(Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
		{
			_response = response ?? throw new ArgumentNullException(nameof(response));
		}

		internal void SetMatchers(IEnumerable<IHttpRequestMatcher> matchers)
		{
			if (matchers == null)
			{
				throw new ArgumentNullException(nameof(matchers));
			}

			_matchers = new ReadOnlyCollection<IHttpRequestMatcher>(matchers.ToList());
		}

		internal void SetCallback(Action<HttpRequestMessage> callback)
		{
			_callback = callback ?? throw new ArgumentNullException(nameof(callback));
		}

		internal void SetVerifiable(string because)
		{
			IsVerifiable = true;
			_verifiableBecause = because;
		}

		public override string ToString()
		{
			return string.Join(" ", Matchers.Select(m => m.ToString()).ToArray());
		}
	}
}