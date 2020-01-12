using System;
using System.Net.Http;

namespace MockHttp.Server
{
	/// <summary>
	/// Extensions for <see cref="MockHttpServer"/>.
	/// </summary>
	public static class MockHttpServerExtensions
	{
		/// <summary>
		/// Creates a <see cref="HttpClient"/> with base address set to URL the specified <paramref name="server"/> is hosting on.
		/// </summary>
		/// <param name="server"></param>
		/// <returns></returns>
		public static HttpClient CreateClient(this MockHttpServer server)
		{
			if (server is null)
			{
				throw new ArgumentNullException(nameof(server));
			}

			if (!server.IsStarted)
			{
				throw new InvalidOperationException("The server must be started first.");
			}

			return new HttpClient
			{
				BaseAddress = new Uri(server.HostUrl)
			};
		}
	}
}
