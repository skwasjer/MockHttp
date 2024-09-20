using System.Text;
using MockHttp.Extensions;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by partially matching the request content.
/// </summary>
public class PartialContentMatcher : ContentMatcher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialContentMatcher" /> class using specified <paramref name="content" />.
    /// </summary>
    /// <param name="content">The request content to match.</param>
    /// <param name="encoding">The content encoding.</param>
    public PartialContentMatcher(string content, Encoding? encoding)
        : base(content, encoding)
    {
        if (ByteContent.Count == 0)
        {
            throw new ArgumentException("Content can not be empty.", nameof(content));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialContentMatcher" /> class using specified raw <paramref name="content" />.
    /// </summary>
    /// <param name="content">The request content to match.</param>
    public PartialContentMatcher(byte[] content)
        : base(content)
    {
        if (ByteContent.Count == 0)
        {
            throw new ArgumentException("Content can not be empty.", nameof(content));
        }
    }

    /// <inheritdoc />
    protected override bool IsMatch(byte[] requestContent)
    {
        return requestContent.Contains(ByteContent);
    }

    /// <inheritdoc />
    public override bool IsExclusive => false;

    /// <inheritdoc />
    public override string ToString()
    {
        return "Partial" + base.ToString();
    }
}
