using System.Data;
using Dapper;
using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Loaders;

public sealed class TruncateInsertLoader<T>(
    IDbConnection connection,
    string table,
    string insertSql,
    int batchSize = 500,
    IDbTransaction? transaction = null,
    ILogger? logger = null) : ILoader<T>
{
    public async Task<int> LoadAsync(IEnumerable<T> records, CancellationToken ct = default)
    {
        var list = records as IReadOnlyList<T> ?? records.ToList();
        logger?.LogInformation("TruncateInsert: clearing {Table}, loading {Count} records", table, list.Count);

        await connection.ExecuteAsync($"DELETE FROM {table}", transaction: transaction);

        var total = 0;
        foreach (var batch in list.Chunk(batchSize))
        {
            ct.ThrowIfCancellationRequested();
            total += await connection.ExecuteAsync(insertSql, batch, transaction: transaction);
            logger?.LogInformation("Inserted batch — {Total}/{Count} rows", total, list.Count);
        }

        logger?.LogInformation("TruncateInsert complete — {Total} rows into {Table}", total, table);
        return total;
    }
}
