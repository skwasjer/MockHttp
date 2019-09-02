using Newtonsoft.Json;

namespace MockHttp.Json
{
	/// <summary>
	/// Configuration extensions.
	/// </summary>
	public static class MockConfigurationExtensions
	{
		/// <summary>
		/// Registers specified <paramref name="serializerSettings"/> as default serializer settings.
		/// </summary>
		/// <param name="config">The mock handler.</param>
		/// <param name="serializerSettings">The serializer settings.</param>
		/// <returns></returns>
		public static IMockConfiguration UseJsonSerializerSettings(this IMockConfiguration config, JsonSerializerSettings serializerSettings)
		{
			return config.Use(serializerSettings);
		}
	}
}
