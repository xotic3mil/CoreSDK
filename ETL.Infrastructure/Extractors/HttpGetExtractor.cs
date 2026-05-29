using System.Runtime.CompilerServices;
using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Extractors;

public sealed class HttpGetExtractor<T>(
    HttpClient httpClient,
    string url,
    Func<HttpResponseMessage, Task<IEnumerable<T>>> responseParser,
    ILogger? logger = null) : IExtractor<T>
{
    public async IAsyncEnumerable<T> ExtractAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        logger?.LogDebug("GET {Url}", url);
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var items = await responseParser(response);
        foreach (var item in items)
            yield return item;
    }
}
