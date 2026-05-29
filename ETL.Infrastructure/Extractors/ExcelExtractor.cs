using System.Runtime.CompilerServices;
using ClosedXML.Excel;
using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Extractors;

public sealed class ExcelExtractor<T> : IExtractor<T> where T : new()
{
    private readonly string _filePath;
    private readonly Func<IXLRow, T> _mapper;
    private readonly int _headerRows;
    private readonly ILogger<ExcelExtractor<T>>? _logger;

    public ExcelExtractor(string filePath, Func<IXLRow, T> mapper, int headerRows = 1, ILogger<ExcelExtractor<T>>? logger = null)
    {
        _filePath = filePath;
        _mapper = mapper;
        _headerRows = headerRows;
        _logger = logger;
    }

    public async IAsyncEnumerable<T> ExtractAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Reading Excel: {File}", _filePath);
        using var workbook = new XLWorkbook(_filePath);
        var sheet = workbook.Worksheets.First();
        foreach (var row in sheet.RowsUsed().Skip(_headerRows))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return _mapper(row);
        }
        await Task.CompletedTask;
    }
}
