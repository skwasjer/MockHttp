using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace MockHttp.Server
{
	/// <summary>
	/// A mock HTTP server that listens on a specific URL and responds according to a configured <see cref="MockHttpHandler"/>.
	/// </summary>
	public sealed class MockHttpServer : IDisposable
	{
		private readonly RequestHandler _requestHandler;
		private readonly IWebHostBuilder _webHostBuilder;
		private IWebHost _host;
		private string _hostUrl;

		/// <summary>
		/// Initializes a new instance of the <see cref="MockHttpServer"/> using specified <paramref name="mockHttpHandler"/>.
		/// </summary>
		/// <param name="mockHttpHandler">The mock http handler.</param>
		public MockHttpServer(MockHttpHandler mockHttpHandler)
		{
			if (mockHttpHandler is null)
			{
				throw new ArgumentNullException(nameof(mockHttpHandler));
			}

			_requestHandler = new RequestHandler(mockHttpHandler);
			_webHostBuilder = new WebHostBuilder()
				.UseKestrel(options => options.AddServerHeader = false)
				.UseEnvironment("Development")
				.Configure(builder => builder.Use(HandleRequest));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MockHttpServer"/> using specified <paramref name="mockHttpHandler"/> and configures it to listen on specified <paramref name="hostUrl"/>.
		/// </summary>
		/// <param name="mockHttpHandler">The mock http handler.</param>
		/// <param name="hostUrl">The host URL the mock HTTP server will listen on.</param>
#pragma warning disable CA1054 // Uri parameters should not be strings
		public MockHttpServer(MockHttpHandler mockHttpHandler, string hostUrl)
#pragma warning restore CA1054 // Uri parameters should not be strings
			: this(mockHttpHandler)
		{
			HostUrl = hostUrl ?? throw new ArgumentNullException(nameof(hostUrl));
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_requestHandler?.Dispose();
			_host?.Dispose();
			_host = null;
		}

		/// <summary>
		/// Gets or sets the host URL the mock HTTP server will listen on.
		/// </summary>
#pragma warning disable CA1056 // Uri properties should not be strings
		public string HostUrl
#pragma warning restore CA1056 // Uri properties should not be strings
		{
			get
			{
				lock (_requestHandler)
				{
					return _host?.ServerFeatures.Get<IServerAddressesFeature>().Addresses.First() ?? _hostUrl;
				}
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
				{
					throw new ArgumentException("", nameof(value));
				}

				_hostUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
				_webHostBuilder.UseUrls(_hostUrl);
			}
		}

		/// <summary>
		/// Gets whether the host is started.
		/// </summary>
		public bool IsStarted => _host != null;

		/// <summary>
		/// Starts listening on the configured addresses.
		/// </summary>
		public Task StartAsync(CancellationToken cancellationToken = default)
		{
			if (_host != null)
			{
				throw new InvalidOperationException($"{nameof(MockHttpServer)} already running.");
			}

			lock (_requestHandler)
			{
				if (_host != null)
				{
					return Task.CompletedTask;
				}

				_host = _webHostBuilder.Build();
				return _host.StartAsync(cancellationToken);
			}
		}

		/// <summary>
		/// Attempt to gracefully stop the mock HTTP server.
		/// </summary>
		public Task StopAsync(CancellationToken cancellationToken = default)
		{
			if (_host == null)
			{
				throw new InvalidOperationException($"{nameof(MockHttpServer)} not running.");
			}

			lock (_requestHandler)
			{
				if (_host == null)
				{
					return Task.CompletedTask;
				}

				// Local copy, so we can null it before disposing.
				IWebHost host = _host;
				_host = null;
				try
				{
					return host.StopAsync(cancellationToken);
				}
				finally
				{
					host.Dispose();
				}
			}
		}

		private async Task HandleRequest(HttpContext httpContext, Func<Task> next)
		{
			if (httpContext is null)
			{
				await next().ConfigureAwait(false);
				return;
			}

			CancellationToken cancellationToken = httpContext.RequestAborted;
			using HttpRequestMessage request = httpContext.Request.AsHttpRequestMessage();

			cancellationToken.ThrowIfCancellationRequested();
			HttpResponseMessage response = await _requestHandler.HandleAsync(request, cancellationToken).ConfigureAwait(false);
			httpContext.Response.RegisterForDispose(response);

			cancellationToken.ThrowIfCancellationRequested();
			IHttpResponseFeature responseFeature = httpContext.Features.Get<IHttpResponseFeature>();
			//IHttpResponseBodyFeature responseBodyFeature = httpContext.Features.Get<IHttpResponseBodyFeature>();
			await response.MapToResponseFeatureAsync(responseFeature, cancellationToken).ConfigureAwait(false);
		}
	}
}
