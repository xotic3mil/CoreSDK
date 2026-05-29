using Dapper;
using ETL.Core.Interfaces;
using ETL.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Logging;

// Uses its own connection per call, independent of the UoW transaction.
// This guarantees log entries are always written, even when the main transaction rolls back.
public sealed class SqlProcessLogger : IProcessLogger
{
    private readonly string _connectionString;
    private readonly ILogger<SqlProcessLogger>? _logger;
    private readonly Dictionary<string, (Guid Id, DateTime StartedAt)> _runs = new();

    public SqlProcessLogger(string connectionString, ILogger<SqlProcessLogger>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task LogStartAsync(string processName, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        _runs[processName] = (id, startedAt);

        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await conn.ExecuteAsync(
            "INSERT INTO process_log (id, process_name, status, started_at) VALUES (@id, @processName, @status, @startedAt)",
            new { id, processName, status = nameof(ProcessStatus.Running), startedAt });

        _logger?.LogInformation("Process {Name} started (run {RunId})", processName, id);
    }

    public async Task LogCompleteAsync(string processName, int recordsProcessed, CancellationToken cancellationToken = default)
    {
        if (!_runs.TryGetValue(processName, out var run)) return;

        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await conn.ExecuteAsync(
            "UPDATE process_log SET status = @status, records_processed = @recordsProcessed, completed_at = @completedAt WHERE id = @id",
            new { id = run.Id, status = nameof(ProcessStatus.Completed), recordsProcessed, completedAt = DateTime.UtcNow });

        _logger?.LogInformation("Process {Name} completed — {Records} records in {Elapsed:F1}s",
            processName, recordsProcessed, (DateTime.UtcNow - run.StartedAt).TotalSeconds);
    }

    public async Task LogErrorAsync(string processName, Exception ex, CancellationToken cancellationToken = default)
    {
        if (!_runs.TryGetValue(processName, out var run)) return;

        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await conn.ExecuteAsync(
            "UPDATE process_log SET status = @status, error_message = @errorMessage, completed_at = @completedAt WHERE id = @id",
            new { id = run.Id, status = nameof(ProcessStatus.Failed), errorMessage = ex.Message, completedAt = DateTime.UtcNow });

        _logger?.LogError(ex, "Process {Name} failed", processName);
    }
}
