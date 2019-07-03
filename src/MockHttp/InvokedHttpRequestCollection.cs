using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MockHttp
{
	internal class InvokedHttpRequestCollection : IInvokedHttpRequestCollection
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly object _syncLock = new object();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<InvokedHttpRequest> _invokedRequests;

		public void Add(InvokedHttpRequest request)
		{
			lock (_syncLock)
			{
				if (_invokedRequests == null)
				{
					_invokedRequests = new List<InvokedHttpRequest>();
				}

				_invokedRequests.Add(request);
			}
		}

		public IEnumerator<IInvokedHttpRequest> GetEnumerator()
		{
			// Take local copies of collection and count so they are isolated from changes by other threads.
			List<InvokedHttpRequest> requests;
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

		public void Clear()
		{
			lock (_syncLock)
			{
				_invokedRequests = null;
			}
		}

		public IInvokedHttpRequest this[int index]
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