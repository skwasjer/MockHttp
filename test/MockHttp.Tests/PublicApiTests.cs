
using MockHttp.Specs;

namespace MockHttp;

public sealed class PublicApiTests : PublicApiSpec
{
    public PublicApiTests()
        : base(typeof(MockHttpHandler))
    {
    }
}
