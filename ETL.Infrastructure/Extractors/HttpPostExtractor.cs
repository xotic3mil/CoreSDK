using System.Runtime.CompilerServices;
using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Extractors;

public sealed class HttpPostExtractor<T>(
    HttpClient httpClient,
    string url,
    Func<HttpContent> bodyFactory,
    Func<HttpResponseMessage, Task<IEnumerable<T>>> responseParser,
    ILogger? logger = null) : IExtractor<T>
{
    public async IAsyncEnumerable<T> ExtractAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        logger?.LogDebug("POST {Url}", url);
        var response = await httpClient.PostAsync(url, bodyFactory(), cancellationToken);
        response.EnsureSuccessStatusCode();
        var items = await responseParser(response);
        foreach (var item in items)
            yield return item;
    }
}
