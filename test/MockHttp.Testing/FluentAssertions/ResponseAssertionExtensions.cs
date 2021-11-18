namespace MockHttp.FluentAssertions
{
	public static class ResponseAssertionExtensions
	{
		public static ResponseAssertions Should(this HttpResponseMessage responseMessage)
		{
			return new ResponseAssertions(responseMessage);
		}
	}
}
