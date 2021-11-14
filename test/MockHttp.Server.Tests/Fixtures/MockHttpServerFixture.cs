using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.TestCorrelator;
using Xunit;
using Xunit.Abstractions;

namespace MockHttp.Fixtures
{
	public class MockHttpServerFixture : IDisposable, IAsyncLifetime
	{
		private ITestCorrelatorContext _testCorrelatorContext;

		public MockHttpServerFixture()
			: this("http")
		{
		}

		protected MockHttpServerFixture(string scheme)
		{
			Logger logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.Enrich.FromLogContext()
				.WriteTo.TestCorrelator()
				.WriteTo.Debug()
				.CreateLogger();
			LoggerFactory = new SerilogLoggerFactory(logger);
			Handler = new MockHttpHandler();
			Server = new MockHttpServer(Handler, LoggerFactory, SupportsIpv6() ? $"{scheme}://[::1]:0" : $"{scheme}://127.0.0.1:0");
			Server
				.Configure(builder => builder
					.Use((httpContext, next) =>
					{
						_testCorrelatorContext ??= TestCorrelator.CreateContext();
						return next();
					})
			);
		}

		public ILoggerFactory LoggerFactory { get; set; }

		public MockHttpHandler Handler { get; }

		public MockHttpServer Server { get; }

		public void Dispose()
		{
			Server?.Dispose();
			Handler?.Dispose();
			GC.SuppressFinalize(this);
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

		// ReSharper disable once MemberCanBeMadeStatic.Global
		public void LogServerTrace(ITestOutputHelper testOutputHelper)
		{
			if (_testCorrelatorContext == null)
			{
				return;
			}

			var logEvents = TestCorrelator.GetLogEventsFromContextGuid(_testCorrelatorContext.Guid).ToList();
			foreach (LogEvent logEvent in logEvents)
			{
				testOutputHelper.WriteLine(logEvent.RenderMessage());
			}
		}

		public void Reset()
		{
			Handler.Reset();
			_testCorrelatorContext = null;
		}
	}
}
