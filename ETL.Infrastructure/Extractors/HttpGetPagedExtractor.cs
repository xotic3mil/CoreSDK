using System.Runtime.CompilerServices;
using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Extractors;

public sealed class HttpGetPagedExtractor<T> : IExtractor<T>
{
    private readonly HttpClient _httpClient;
    private readonly Func<int, string> _urlFactory;
    private readonly Func<HttpResponseMessage, Task<(IEnumerable<T> Items, bool HasMore)>> _responseParser;
    private readonly ILogger<HttpGetPagedExtractor<T>>? _logger;

    public HttpGetPagedExtractor(
        HttpClient httpClient,
        Func<int, string> urlFactory,
        Func<HttpResponseMessage, Task<(IEnumerable<T> Items, bool HasMore)>> responseParser,
        ILogger<HttpGetPagedExtractor<T>>? logger = null)
    {
        _httpClient = httpClient;
        _urlFactory = urlFactory;
        _responseParser = responseParser;
        _logger = logger;
    }

    public async IAsyncEnumerable<T> ExtractAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        bool hasMore;
        do
        {
            var url = _urlFactory(page);
            _logger?.LogDebug("Fetching page {Page}: {Url}", page, url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var (items, more) = await _responseParser(response);
            foreach (var item in items)
                yield return item;
            hasMore = more;
            page++;
        } while (hasMore);
    }
}
