using System;
using MockHttp.Threading;

namespace MockHttp
{
	internal class InvokedHttpRequestCollection : ConcurrentCollection<IInvokedHttpRequest>, IInvokedHttpRequestCollection
	{
		private readonly MockHttpHandler _owner;

		public InvokedHttpRequestCollection(MockHttpHandler owner)
		{
			_owner = owner ?? throw new ArgumentNullException(nameof(owner));
		}

		void IInvokedHttpRequestCollection.Clear()
		{
			_owner.UninvokeAll();
			base.Clear();
		}
	}
}