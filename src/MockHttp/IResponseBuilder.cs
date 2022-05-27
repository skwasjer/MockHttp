﻿using System.ComponentModel;
using MockHttp.Language.Response;

namespace MockHttp;

/// <summary>
/// A builder to compose HTTP responses via a behavior pipeline.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IResponseBuilder
    : IWithResponse,
      IWithStatusCode,
      IWithContent,
      IWithHeaders,
      IFluentInterface
{
}
