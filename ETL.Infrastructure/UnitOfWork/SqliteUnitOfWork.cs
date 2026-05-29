using System.Data;
using ETL.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace ETL.Infrastructure.UnitOfWork;

public sealed class SqliteUnitOfWork : IUnitOfWork
{
    private readonly SqliteConnection _connection;
    private readonly SqliteTransaction _transaction;
    private bool _disposed;

    private SqliteUnitOfWork(SqliteConnection connection, SqliteTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public static async Task<SqliteUnitOfWork> CreateAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        return new SqliteUnitOfWork(connection, transaction);
    }

    public IDbConnection Connection => _connection;
    public IDbTransaction Transaction => _transaction;

    public async Task CommitAsync(CancellationToken cancellationToken = default)
        => await _transaction.CommitAsync(cancellationToken);

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
        => await _transaction.RollbackAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
