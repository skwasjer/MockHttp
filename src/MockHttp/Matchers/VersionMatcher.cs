using System;
using MockHttp.Responses;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the HTTP message version.
	/// </summary>
	public class VersionMatcher : ValueMatcher<Version>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VersionMatcher"/> class using specified <paramref name="version"/>.
		/// </summary>
		/// <param name="version"></param>
		public VersionMatcher(Version version) : base(version ?? throw new ArgumentNullException(nameof(version)))
		{
		}

		/// <inheritdoc />
		public override bool IsMatch(MockHttpRequestContext requestContext)
		{
			return requestContext.Request.Version.Equals(Value);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Version: {Value}";
		}

		/// <inheritdoc />
		public override bool IsExclusive => true;
	}
}
