using System.Text.Json;
using Dapper;
using ETL.Core.Interfaces;
using ETL.Core.Models;
using Microsoft.Data.Sqlite;

namespace ETL.Infrastructure.DeadLetter;

// Uses its own connection so dead-letter writes survive a rolled-back transaction.
public sealed class SqlDeadLetterStore : IDeadLetterStore
{
    private readonly string _connectionString;

    public SqlDeadLetterStore(string connectionString) => _connectionString = connectionString;

    public async Task WriteAsync<T>(string processName, T record, Exception ex, CancellationToken cancellationToken = default)
    {
        var entry = new DeadLetterEntry
        {
            ProcessName = processName,
            Payload = JsonSerializer.Serialize(record),
            ErrorMessage = ex.Message
        };

        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await conn.ExecuteAsync(
            "INSERT INTO dead_letters (id, process_name, payload, error_message, failed_at) VALUES (@Id, @ProcessName, @Payload, @ErrorMessage, @FailedAt)",
            entry);
    }
}
