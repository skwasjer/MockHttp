using MockHttp.Language;

namespace MockHttp;

/// <summary>
/// Extensions for <see cref="IRequestMatching" />.
/// </summary>
public static class AuthenticationRequestMatchingExtensions
{
    /// <summary>
    /// Matches a request when it has an Authorization header with a bearer token.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRequestMatching BearerToken(this IRequestMatching builder)
    {
        return builder.Header("Authorization", "Bearer *", true);
    }

    /// <summary>
    /// Matches a request when it has an Authorization header with specified bearer <paramref name="token" />.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="token">The bearer token (without 'Bearer').</param>
    /// <returns></returns>
    public static IRequestMatching BearerToken(this IRequestMatching builder, string token)
    {
        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        return builder.Header("Authorization", $"Bearer {token}");
    }
}
