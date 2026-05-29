using ETL.Core.Interfaces;
using ETL.Infrastructure.SqlGeneration;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Loaders;

public sealed class BulkLoaderFactory(ILoggerFactory loggerFactory) : IBulkLoaderFactory
{
    // Auto-SQL overloads
    public ILoader<T> CreateTruncateInsert<T>(IUnitOfWork uow, int batchSize = 500) =>
        CreateTruncateInsert<T>(uow, SqlMapper<T>.TableName, SqlMapper<T>.InsertSql, batchSize);

    public ILoader<T> CreateAppend<T>(IUnitOfWork uow, int batchSize = 500) =>
        CreateAppend<T>(uow, SqlMapper<T>.InsertSql, batchSize);

    public ILoader<T> CreateUpsertDelete<T>(IUnitOfWork uow, int batchSize = 500) =>
        CreateUpsertDelete<T>(uow, SqlMapper<T>.TableName, SqlMapper<T>.KeyColumn, SqlMapper<T>.KeySelector, SqlMapper<T>.UpsertSql, batchSize);

    // Manual-SQL overloads
    public ILoader<T> CreateTruncateInsert<T>(IUnitOfWork uow, string table, string insertSql, int batchSize = 500) =>
        new TruncateInsertLoader<T>(
            uow.Connection, table, insertSql, batchSize,
            transaction: uow.Transaction,
            logger: loggerFactory.CreateLogger<TruncateInsertLoader<T>>());

    public ILoader<T> CreateAppend<T>(IUnitOfWork uow, string insertSql, int batchSize = 500) =>
        new AppendLoader<T>(
            uow.Connection, insertSql, batchSize,
            transaction: uow.Transaction,
            logger: loggerFactory.CreateLogger<AppendLoader<T>>());

    public ILoader<T> CreateUpsertDelete<T>(IUnitOfWork uow, string table, string keyColumn, Func<T, object> keySelector, string upsertSql, int batchSize = 500) =>
        new UpsertDeleteLoader<T>(
            uow.Connection, table, keyColumn, keySelector, upsertSql, batchSize,
            transaction: uow.Transaction,
            logger: loggerFactory.CreateLogger<UpsertDeleteLoader<T>>());
}
