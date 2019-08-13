using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp
{
	internal class HttpCallSequence : HttpCall
	{
		private int _requestIndex;
		private readonly List<Func<HttpRequestMessage, Task<HttpResponseMessage>>> _responseSequence;

		public HttpCallSequence()
		{
			_responseSequence = new List<Func<HttpRequestMessage, Task<HttpResponseMessage>>>();
			_requestIndex = -1;
		}

		public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			int nextRequestIndex = IncrementIfLessThan(ref _requestIndex, _responseSequence.Count - 1);

			// TODO: if Reset() has been called from other thread, this can result in IndexOutOfRangeException. Have to handle this is some way and make it thread safe.
			Func<HttpRequestMessage, Task<HttpResponseMessage>> response = nextRequestIndex >= 0
				? _responseSequence[nextRequestIndex]
				: null;

			if (response == null)
			{
				// TODO: clarify which mock.
				throw new HttpMockException("No response configured for mock.");
			}

			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				Callback?.Invoke(request);
				HttpResponseMessage responseMessage = await response(request).ConfigureAwait(false);
				responseMessage.RequestMessage = request;
				return responseMessage;
			}
			finally
			{
				IsInvoked = true;
			}
		}

		public override void SetResponse(Func<HttpRequestMessage, Task<HttpResponseMessage>> response)
		{
			_responseSequence.Add(response);
		}

		public override bool VerifyIfInvoked()
		{
			if (IsInvoked && _requestIndex == _responseSequence.Count - 1)
			{
				IsVerified = true;
			}

			return IsVerified;
		}

		public override void Reset()
		{
			_responseSequence.Clear();
			_requestIndex = -1;

			base.Reset();
		}

		/// <summary>
		/// Thread safe increment of an integer while less than <paramref name="comparand"/>.
		/// </summary>
		/// <returns>The incremented value or if equal/greater the original value.</returns>
		private static int IncrementIfLessThan(ref int location, int comparand)
		{
			int initialValue;
			int newValue;
			do
			{
				initialValue = location;
				newValue = location + 1;
				if (initialValue >= comparand)
				{
					return initialValue;
				}
			}
			while (Interlocked.CompareExchange(ref location, newValue, initialValue) != initialValue);
			return newValue;
		}
	}
}