namespace MockHttp;

public sealed class XunitAppDomainUnloadHook
{
    [Fact]
    public void Unload()
    {
        AppDomain.CurrentDomain.DomainUnload += (_, _) =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        };
        Assert.True(true);
    }
}
