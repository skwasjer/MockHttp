namespace MockHttp;

internal static class HttpContentExtensions
{
    /// <summary>
    /// Creates a shallow clone of <see cref="HttpContent" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <remarks>This does not create a true clone, because inherited content could have extra properties, and we're not using reflection.</remarks>
    public static async Task<ByteArrayContent> CloneAsByteArrayContentAsync<T>(this T content)
        where T : HttpContent
    {
        if (content == default)
        {
            return default;
        }

        await Task.Yield();

        await content.LoadIntoBufferAsync().ConfigureAwait(false);

        using var ms = new MemoryStream();
        await content.CopyToAsync(ms).ConfigureAwait(false);
        var clone = new ByteArrayContent(ms.ToArray());

        if (content.Headers is null)
        {
            return clone;
        }

        foreach (KeyValuePair<string, IEnumerable<string>> h in content.Headers)
        {
            clone.Headers.Add(h.Key, h.Value);
        }

        return clone;
    }
}
