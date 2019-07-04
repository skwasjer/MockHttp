using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp
{
	/// <summary>
	/// Contains the setup that controls the behavior of a mocked HTTP request.
	/// </summary>
	internal sealed class HttpCall
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

		public bool IsVerifiable { get; private set; }

		public bool IsVerified { get; private set; }

		public bool IsInvoked { get; private set; }

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			IsInvoked = true;

			if (_response == null)
			{
				// TODO: clarify which mock.
				throw new HttpMockException("No response configured for mock.");
			}

			cancellationToken.ThrowIfCancellationRequested();

			_callback?.Invoke(request);
			HttpResponseMessage responseMessage = await _response(request).ConfigureAwait(false);
			responseMessage.RequestMessage = request;
			return responseMessage;
		}

		public void SetResponse(Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
		{
			_response = response ?? throw new ArgumentNullException(nameof(response));
		}

		public void SetMatchers(IEnumerable<IHttpRequestMatcher> matchers)
		{
			if (matchers == null)
			{
				throw new ArgumentNullException(nameof(matchers));
			}

			_matchers = new ReadOnlyCollection<IHttpRequestMatcher>(matchers.ToList());
		}

		public void SetCallback(Action<HttpRequestMessage> callback)
		{
			_callback = callback ?? throw new ArgumentNullException(nameof(callback));
		}

		public void SetVerifiable(string because)
		{
			IsVerifiable = true;
			_verifiableBecause = because;
		}

		public override string ToString()
		{
			if (_matchers == null || _matchers.Count == 0)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			foreach (IHttpRequestMatcher m in _matchers)
			{
				sb.Append(m);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}

		public bool VerifyIfInvoked()
		{
			if (IsInvoked)
			{
				IsVerified = true;
			}

			return IsVerified;
		}

		public void Reset()
		{
			IsInvoked = false;
			IsVerified = false;
			IsVerifiable = false;
			_verifiableBecause = null;
			_response = null;
			_callback = null;
			_matchers = null;
		}
	}
}