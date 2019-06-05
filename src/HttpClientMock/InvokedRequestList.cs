using System;
using System.Collections;
using System.Collections.Generic;

namespace HttpClientMock
{
	internal class InvokedRequestList : IInvokedRequestList
	{
		private readonly object _syncLock = new object();
		private List<IMockedHttpRequest> _invokedRequests;

		public void Add(IMockedHttpRequest request)
		{
			lock (_syncLock)
			{
				if (_invokedRequests == null)
				{
					_invokedRequests = new List<IMockedHttpRequest>();
				}

				_invokedRequests.Add(request);
			}
		}

		public IEnumerator<IMockedHttpRequest> GetEnumerator()
		{
			// Take local copies of collection and count so they are isolated from changes by other threads.
			List<IMockedHttpRequest> requests;
			int count;

			lock (_syncLock)
			{
				if (_invokedRequests == null)
				{
					yield break;
				}

				requests = _invokedRequests;
				count = _invokedRequests.Count;
			}

			for (int i = 0; i < count; i++)
			{
				yield return requests[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count
		{
			get
			{
				lock (_syncLock)
				{
					return _invokedRequests?.Count ?? 0;
				}
			}
		}

		public void Reset()
		{
			lock (_syncLock)
			{
				_invokedRequests = null;
			}
		}

		public IMockedHttpRequest this[int index]
		{
			get
			{
				lock (_syncLock)
				{
					if (_invokedRequests == null)
					{
						throw new IndexOutOfRangeException();
					}

					return _invokedRequests[index];
				}
			}
		}
	}
}