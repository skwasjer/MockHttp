using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MockHttp.Responses;

namespace MockHttp
{
	internal class HttpCallSequence : HttpCall
	{
		private int _requestIndex;
		private readonly List<IResponseStrategy> _responseSequence;

		public HttpCallSequence()
		{
			_responseSequence = new List<IResponseStrategy>();
			_requestIndex = -1;
		}

		public override async Task<HttpResponseMessage> SendAsync(MockHttpRequestContext requestContext, CancellationToken cancellationToken)
		{
			int nextRequestIndex = IncrementIfLessThan(ref _requestIndex, _responseSequence.Count - 1);

			// TODO: if Reset() has been called from other thread, this can result in IndexOutOfRangeException. Have to handle this is some way and make it thread safe.
			IResponseStrategy responseStrategy = nextRequestIndex >= 0
				? _responseSequence[nextRequestIndex]
				: null;

			if (responseStrategy is null)
			{
				// TODO: clarify which mock.
				throw new HttpMockException("No response configured for mock.");
			}

			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				Callback?.Invoke(requestContext.Request);
				HttpResponseMessage responseMessage = await responseStrategy.ProduceResponseAsync(requestContext, cancellationToken).ConfigureAwait(false);
				responseMessage.RequestMessage = requestContext.Request;
				return responseMessage;
			}
			finally
			{
				IsInvoked = true;
			}
		}

		public override void SetResponse(IResponseStrategy responseStrategy)
		{
			_responseSequence.Add(responseStrategy);
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
			base.Reset();
		}

		public override void Uninvoke()
		{
			Interlocked.Exchange(ref _requestIndex, -1);
			base.Uninvoke();
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
				newValue = initialValue + 1;
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
