using Dapper;
using ETL.Core.Interfaces;
using ETL.Core.Models;
using Microsoft.Data.Sqlite;

namespace ETL.Infrastructure.Watermarking;

public sealed class SqlWatermarkStore(string connectionString) : IWatermarkStore
{
    public async Task<WatermarkEntry?> GetAsync(string processName, string entityName, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        return await conn.QuerySingleOrDefaultAsync<WatermarkEntry>("""
            SELECT process_name AS ProcessName,
                   entity_name  AS EntityName,
                   last_run_at  AS LastRunAt,
                   last_value   AS LastValue
            FROM watermarks
            WHERE process_name = @processName
              AND entity_name  = @entityName
            """, new { processName, entityName });
    }

    public async Task SetAsync(WatermarkEntry entry, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        await conn.ExecuteAsync("""
            INSERT INTO watermarks (process_name, entity_name, last_run_at, last_value)
            VALUES (@ProcessName, @EntityName, @LastRunAt, @LastValue)
            ON CONFLICT (process_name, entity_name)
            DO UPDATE SET last_run_at = EXCLUDED.last_run_at,
                          last_value  = EXCLUDED.last_value
            """, entry);
    }

    public async Task<IReadOnlyList<WatermarkEntry>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        var rows = await conn.QueryAsync<WatermarkEntry>("""
            SELECT process_name AS ProcessName,
                   entity_name  AS EntityName,
                   last_run_at  AS LastRunAt,
                   last_value   AS LastValue
            FROM watermarks
            ORDER BY process_name, entity_name
            """);
        return rows.ToList();
    }

    public async Task DeleteByProcessAsync(string processName, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        await conn.ExecuteAsync(
            "DELETE FROM watermarks WHERE process_name = @processName",
            new { processName });
    }
}
