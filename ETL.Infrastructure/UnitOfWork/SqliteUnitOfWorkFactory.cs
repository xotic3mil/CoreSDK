using ETL.Core.Interfaces;

namespace ETL.Infrastructure.UnitOfWork;

public sealed class SqliteUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly string _connectionString;

    public SqliteUnitOfWorkFactory(string connectionString) => _connectionString = connectionString;

    public async Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = default)
        => await SqliteUnitOfWork.CreateAsync(_connectionString, cancellationToken);
}
