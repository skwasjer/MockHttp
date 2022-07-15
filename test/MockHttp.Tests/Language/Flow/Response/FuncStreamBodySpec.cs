﻿#nullable enable
using FluentAssertions;
using MockHttp.FluentAssertions;

namespace MockHttp.Language.Flow.Response;

public class FuncStreamBodySpec : ByteBodySpec
{
    protected override void Given(IResponseBuilder with)
    {
        with.Body(() => new MemoryStream(Content));
    }

    protected override Task Should(HttpResponseMessage response)
    {
        return response
            .Should()
            .HaveContentType(MediaTypes.OctetStream)
            .And.HaveContentAsync(Content);
    }
}
#nullable restore