using System.Data;
using Dapper;
using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Loaders;

public sealed class AppendLoader<T>(
    IDbConnection connection,
    string insertSql,
    int batchSize = 500,
    IDbTransaction? transaction = null,
    ILogger? logger = null) : ILoader<T>
{
    public async Task<int> LoadAsync(IEnumerable<T> records, CancellationToken ct = default)
    {
        var list = records as IReadOnlyList<T> ?? records.ToList();
        logger?.LogInformation("Appending {Count} records in batches of {BatchSize}", list.Count, batchSize);

        var total = 0;
        foreach (var batch in list.Chunk(batchSize))
        {
            ct.ThrowIfCancellationRequested();
            total += await connection.ExecuteAsync(insertSql, batch, transaction: transaction);
            logger?.LogInformation("Inserted batch — {Total}/{Count} rows", total, list.Count);
        }

        logger?.LogInformation("Append complete — {Total} rows", total);
        return total;
    }
}
