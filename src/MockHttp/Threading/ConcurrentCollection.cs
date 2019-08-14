using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MockHttp.Threading
{
	internal class ConcurrentCollection<T> : IConcurrentReadOnlyList<T>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly object _syncLock = new object();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<T> _items;

		public void Add(T request)
		{
			lock (_syncLock)
			{
				if (_items == null)
				{
					_items = new List<T>();
				}

				_items.Add(request);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			// Take local copies of collection and count so they are isolated from changes by other threads.
			List<T> items;
			int count;

			lock (_syncLock)
			{
				if (_items == null)
				{
					yield break;
				}

				items = _items;
				count = _items.Count;
			}

			for (int i = 0; i < count; i++)
			{
				yield return items[i];
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
					return _items?.Count ?? 0;
				}
			}
		}

		public void Clear()
		{
			lock (_syncLock)
			{
				_items = null;
			}
		}

		public T this[int index]
		{
			get
			{
				lock (_syncLock)
				{
					if (_items == null)
					{
						throw new IndexOutOfRangeException();
					}

					return _items[index];
				}
			}
		}
	}
}