using System.Diagnostics;
using MockHttp.Http;
using MockHttp.Responses;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by the posted form data.
/// </summary>
public class FormDataMatcher : IAsyncHttpRequestMatcher
{
    internal const string FormUrlEncodedMediaType = "application/x-www-form-urlencoded";
    internal const string MultipartFormDataMediaType = "multipart/form-data";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Dictionary<string, IEnumerable<string>> _matchQs;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormDataMatcher" /> class using specified form data parameters.
    /// </summary>
    /// <param name="formData">The form data parameters.</param>
    public FormDataMatcher(IEnumerable<KeyValuePair<string, IEnumerable<string>>> formData)
    {
        if (formData is null)
        {
            throw new ArgumentNullException(nameof(formData));
        }

        _matchQs = formData.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value?.Where(v => v is not null) ?? new List<string>()
        );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormDataMatcher" /> class using specified form data.
    /// </summary>
    /// <param name="urlEncodedFormData">The form data.</param>
    public FormDataMatcher(string urlEncodedFormData)
        : this(DataEscapingHelper.Parse(urlEncodedFormData ?? throw new ArgumentNullException(nameof(urlEncodedFormData))))
    {
    }

    /// <inheritdoc />
    public virtual async Task<bool> IsMatchAsync(MockHttpRequestContext requestContext)
    {
        if (requestContext is null)
        {
            throw new ArgumentNullException(nameof(requestContext));
        }

        if (!CanProcessContent(requestContext.Request.Content))
        {
            return false;
        }

        IDictionary<string, IEnumerable<string>> formData = await GetFormDataAsync(requestContext.Request.Content).ConfigureAwait(false);

        // When match collection is empty, behavior is flipped, and we expect no form data parameters on request.
        if (_matchQs.Count == 0 && formData.Count > 0)
        {
            return false;
        }

        return _matchQs.All(q => formData.ContainsKey(q.Key)
         && (
            BothAreEmpty(formData[q.Key], q.Value)
         || HasOneOf(formData[q.Key], q.Value))
        );
    }

    private static bool BothAreEmpty(IEnumerable<string> left, IEnumerable<string> right)
    {
        int leftCount = left.Count();
        int rightCount = right.Count();
        return leftCount == 0 && leftCount == rightCount;
    }

    private static bool HasOneOf(IEnumerable<string> left, IEnumerable<string> right)
    {
        return left.Any(right.Contains);
    }

    /// <inheritdoc />
    public virtual bool IsExclusive => _matchQs.Count == 0;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"FormData: '{DataEscapingHelper.Format(_matchQs)}'";
    }

    private static async Task<IDictionary<string, IEnumerable<string>>> GetFormDataAsync(HttpContent content)
    {
        if (content is MultipartFormDataContent multipartFormDataContent)
        {
            var formData = new List<KeyValuePair<string, string>>();
            foreach (HttpContent httpContent in multipartFormDataContent
                         .Where(c => c.Headers.ContentDisposition is not null && c.Headers.ContentDisposition.DispositionType == "form-data" && !string.IsNullOrEmpty(c.Headers.ContentDisposition.Name))
                    )
            {
                string key = httpContent.Headers.ContentDisposition.Name;
                bool isFileUpload = httpContent.Headers.ContentType is not null && !string.IsNullOrEmpty(httpContent.Headers.ContentDisposition.FileName);
                if (isFileUpload)
                {
                    // TODO: Support file uploads? Maybe using different matcher, to support (large) streams.
                }
                else
                {
                    // Multipart form data is not escaped, so just read content as string.
                    formData.Add(
                        new KeyValuePair<string, string>(
                            key,
                            await httpContent.ReadAsStringAsync().ConfigureAwait(false)
                        )
                    );
                }
            }

            return formData
                .GroupBy(fd => fd.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Select(s => s.Value));
        }

        string rawFormData = await content.ReadAsStringAsync().ConfigureAwait(false);
        return DataEscapingHelper.Parse(rawFormData).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private static bool CanProcessContent(HttpContent httpContent)
    {
        return httpContent?.Headers.ContentType is not null && IsFormData(httpContent.Headers.ContentType.MediaType);
    }

    private static bool IsFormData(string mediaType)
    {
        return mediaType == FormUrlEncodedMediaType || mediaType == MultipartFormDataMediaType;
    }
}
