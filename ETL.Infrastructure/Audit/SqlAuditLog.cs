using Dapper;
using ETL.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace ETL.Infrastructure.Audit;

public sealed class SqlAuditLog(string connectionString) : IAuditLog
{
    public async Task LogAsync(string? processName, string action, string? detail = null, CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        await conn.ExecuteAsync(
            "INSERT INTO audit_log (id, process_name, action, detail, occurred_at) VALUES (@id, @processName, @action, @detail, @occurredAt)",
            new { id = Guid.NewGuid(), processName, action, detail, occurredAt = DateTime.UtcNow });
    }
}
