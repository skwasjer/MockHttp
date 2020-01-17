using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MockHttp.Server
{
	public class MockHttpServerFixture : IDisposable, IAsyncLifetime
	{
		public MockHttpServerFixture()
			: this("http")
		{
		}

		protected MockHttpServerFixture(string scheme)
		{
			LoggerFactory = new LoggerFactory();
			Handler = new MockHttpHandler();
			Server = new MockHttpServer(Handler, LoggerFactory, SupportsIpv6() ? $"{scheme}://[::1]:0" : $"{scheme}://127.0.0.1:0");
		}

		public ILoggerFactory LoggerFactory { get; set; }

		public MockHttpHandler Handler { get; }

		public MockHttpServer Server { get; }

		public void Dispose()
		{
			Server?.Dispose();
			Handler?.Dispose();
		}

		public Task InitializeAsync()
		{
			return Server.StartAsync();
		}

		public Task DisposeAsync()
		{
			return Server.StopAsync();
		}

		private static bool SupportsIpv6()
		{
			NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			return networkInterfaces.Any(ni => ni.Supports(NetworkInterfaceComponent.IPv6));
		}
	}
}
