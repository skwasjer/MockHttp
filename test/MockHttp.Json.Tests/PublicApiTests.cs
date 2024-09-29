using MockHttp.Specs;

namespace MockHttp.Json;

public sealed class PublicApiTests : PublicApiSpec
{
    public PublicApiTests()
        : base(typeof(IJsonAdapter))
    {
    }
}
