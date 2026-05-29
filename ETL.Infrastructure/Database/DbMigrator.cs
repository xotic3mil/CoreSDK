using Dapper;
using DbUp;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Database;

public sealed class DbMigrator(string connectionString, ILogger<DbMigrator>? logger = null)
{
    private readonly string _connectionString = connectionString;
    private readonly ILogger<DbMigrator>? _logger = logger;

    public void Migrate(string scriptPath)
    {
        var upgrader = DeployChanges.To
            .SqliteDatabase(_connectionString)
            .WithScriptsFromFileSystem(scriptPath)
            .WithTransaction()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            _logger?.LogError(result.Error, "Database migration failed");
            throw result.Error;
        }

        _logger?.LogInformation("Database migration completed successfully");
    }

    public async Task CleanupStaleRunsAsync(CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(ct);
        var affected = await conn.ExecuteAsync("""
            UPDATE process_log
            SET status        = 'Failed',
                error_message = 'Process was interrupted (Worker restarted)',
                completed_at  = @now
            WHERE status = 'Running'
            """, new { now = DateTime.UtcNow });

        if (affected > 0)
            _logger?.LogWarning("Marked {Count} interrupted run(s) as failed on startup", affected);
    }
}
