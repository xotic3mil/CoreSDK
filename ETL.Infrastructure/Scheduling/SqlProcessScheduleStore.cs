using Dapper;
using ETL.Core.Interfaces;
using ETL.Core.Models;
using Microsoft.Data.Sqlite;

namespace ETL.Infrastructure.Scheduling;

public sealed class SqlProcessScheduleStore(string connectionString) : IProcessScheduleStore
{
    public async Task EnsureRegisteredAsync(string name, string? createdBy = null, CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO process_schedule (name, cron, enabled, modified_at, created_by, created_at)
            VALUES (@name, NULL, 0, @now, @createdBy, @now)
            ON CONFLICT (name) DO UPDATE SET
                created_by = CASE WHEN process_schedule.created_by IS NULL THEN excluded.created_by ELSE process_schedule.created_by END,
                created_at = CASE WHEN process_schedule.created_at IS NULL THEN excluded.created_at ELSE process_schedule.created_at END
            """,
            new { name, now = DateTime.UtcNow, createdBy });
    }

    public async Task<ProcessSchedule?> GetAsync(string name, CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<ProcessSchedule>(
            """
            SELECT name, cron, enabled, description, modified_at AS modifiedat,
                   created_by AS createdby, created_at AS createdat
            FROM process_schedule
            WHERE name = @name
            """,
            new { name });
    }

    public async Task<IReadOnlyList<ProcessSchedule>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        var rows = await conn.QueryAsync<ProcessSchedule>(
            "SELECT name, cron, enabled, description, modified_at AS modifiedat, created_by AS createdby, created_at AS createdat FROM process_schedule ORDER BY name");
        return [..rows];
    }

    public async Task SaveAsync(string name, string? cron, bool enabled, string? description = null, CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        await conn.ExecuteAsync(
            """
            UPDATE process_schedule
            SET cron = @cron, enabled = @enabled, description = @description, modified_at = @now
            WHERE name = @name
            """,
            new { name, cron, enabled, description, now = DateTime.UtcNow });
    }
}
