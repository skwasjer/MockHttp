namespace MockHttp.Language.Flow.Response;

public class TransferRateWithMultipleRequestsSpec : TransferRateSpec2
{
    protected override async Task<HttpResponseMessage> When(HttpClient httpClient)
    {
        // Perform 5 requests in parallel.
        HttpResponseMessage[] result = await Task.WhenAll(
            base.When(httpClient),
            base.When(httpClient),
            base.When(httpClient),
            base.When(httpClient),
            base.When(httpClient)
        );

        return result.Last();
    }
}
