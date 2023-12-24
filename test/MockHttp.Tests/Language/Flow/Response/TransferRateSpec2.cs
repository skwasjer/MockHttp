using static MockHttp.BitRate;
using static MockHttp.NetworkLatency;

namespace MockHttp.Language.Flow.Response;

public class TransferRateSpec2 : TransferRateSpec
{
    protected override void Given(IResponseBuilder with)
    {
        with
            .Body(Content)
            .Latency(FourG)
            .TransferRate(() => FromInt32(BitRate));
    }
}
