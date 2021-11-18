namespace MockHttp.Fixtures;

public class MockHttpsServerFixture : MockHttpServerFixture
{
	public MockHttpsServerFixture()
		: base("https")
	{
	}
}