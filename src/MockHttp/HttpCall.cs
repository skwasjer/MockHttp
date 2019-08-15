using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MockHttp.Matchers;

namespace MockHttp
{
	/// <summary>
	/// Contains the setup that controls the behavior of a mocked HTTP request.
	/// </summary>
	internal class HttpCall
	{
		private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _response;
		private string _verifiableBecause;
		private IReadOnlyCollection<HttpRequestMatcher> _matchers;

		public IReadOnlyCollection<HttpRequestMatcher> Matchers
		{
			get
			{
				if (_matchers == null)
				{
					SetMatchers(new List<HttpRequestMatcher>());
				}

				return _matchers;
			}
		}

		public virtual bool IsVerifiable { get; private set; }

		public virtual bool IsVerified { get; protected set; }

		public virtual bool IsInvoked { get; protected set; }

		public Action<HttpRequestMessage> Callback { get; private set; }

		public virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			IsInvoked = true;

			if (_response == null)
			{
				// TODO: clarify which mock.
				throw new HttpMockException("No response configured for mock.");
			}

			cancellationToken.ThrowIfCancellationRequested();

			Callback?.Invoke(request);
			HttpResponseMessage responseMessage = await _response(request, cancellationToken).ConfigureAwait(false);
			responseMessage.RequestMessage = request;
			return responseMessage;
		}

		public virtual void SetResponse(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> response)
		{
			_response = response ?? throw new ArgumentNullException(nameof(response));
		}

		public virtual void SetMatchers(IEnumerable<HttpRequestMatcher> matchers)
		{
			if (matchers == null)
			{
				throw new ArgumentNullException(nameof(matchers));
			}

			_matchers = new ReadOnlyCollection<HttpRequestMatcher>(matchers.ToList());
		}

		public virtual void SetCallback(Action<HttpRequestMessage> callback)
		{
			Callback = callback ?? throw new ArgumentNullException(nameof(callback));
		}

		public virtual void SetVerifiable(string because)
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
			foreach (HttpRequestMatcher m in _matchers)
			{
				sb.Append(m);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}

		public virtual bool VerifyIfInvoked()
		{
			if (IsInvoked)
			{
				IsVerified = true;
			}

			return IsVerified;
		}

		public virtual void Reset()
		{
			IsInvoked = false;
			IsVerified = false;
			IsVerifiable = false;
			_verifiableBecause = null;
			_response = null;
			Callback = null;
			_matchers = null;
		}
	}
}