#nullable enable
namespace MockHttp.Specs;

public abstract class GuardedResponseSpec : ResponseSpec
{
    protected override async Task<HttpResponseMessage> When(HttpClient httpClient)
    {
        await ShouldThrow(() => base.When(httpClient));
        return null!;
    }

    protected sealed override Task Should(HttpResponseMessage response)
    {
        return Task.CompletedTask;
    }

    protected abstract Task ShouldThrow(Func<Task> act);
}
#nullable restore
