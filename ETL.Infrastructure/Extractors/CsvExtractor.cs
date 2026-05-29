using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Extractors;

public sealed class CsvExtractor<T> : IExtractor<T>
{
    private readonly string _filePath;
    private readonly CsvConfiguration _config;
    private readonly ILogger<CsvExtractor<T>>? _logger;

    public CsvExtractor(string filePath, CsvConfiguration? config = null, ILogger<CsvExtractor<T>>? logger = null)
    {
        _filePath = filePath;
        _config = config ?? new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
        _logger = logger;
    }

    public async IAsyncEnumerable<T> ExtractAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Reading CSV: {File}", _filePath);
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, _config);
        await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
            yield return record;
    }
}
