namespace MockHttp.Json;

/// <summary>
/// To deal with runtime API differences around mocking with nullable.
/// </summary>
internal static class ArgAny
{
#if NETCOREAPP3_1_OR_GREATER
    public static ref string? String() => ref Arg.Any<string?>();
#else
    public static ref string String() => ref Arg.Any<string>();
#endif
}
