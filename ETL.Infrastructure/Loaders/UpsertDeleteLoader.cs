using System.Data;
using Dapper;
using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Loaders;

public sealed class UpsertDeleteLoader<T>(
    IDbConnection connection,
    string table,
    string keyColumn,
    Func<T, object> keySelector,
    string upsertSql,
    int batchSize = 500,
    IDbTransaction? transaction = null,
    ILogger? logger = null) : ILoader<T>
{
    public async Task<int> LoadAsync(IEnumerable<T> records, CancellationToken ct = default)
    {
        var list = records as IReadOnlyList<T> ?? records.ToList();
        logger?.LogInformation("UpsertDelete: syncing {Count} records into {Table}", list.Count, table);

        var total = 0;
        foreach (var batch in list.Chunk(batchSize))
        {
            ct.ThrowIfCancellationRequested();
            total += await connection.ExecuteAsync(upsertSql, batch, transaction: transaction);
            logger?.LogInformation("Upserted batch — {Total}/{Count} rows", total, list.Count);
        }

        var sourceKeys = list.Select(keySelector).ToList();
        int deleted;
        if (sourceKeys.Count == 0)
        {
            deleted = await connection.ExecuteAsync(
                $"DELETE FROM {table}",
                transaction: transaction);
        }
        else
        {
            deleted = await connection.ExecuteAsync(
                $"DELETE FROM {table} WHERE {keyColumn} NOT IN @Keys",
                new { Keys = sourceKeys },
                transaction: transaction);
        }

        logger?.LogInformation("UpsertDelete complete — {Total} upserted, {Deleted} deleted from {Table}", total, deleted, table);
        return total;
    }
}
