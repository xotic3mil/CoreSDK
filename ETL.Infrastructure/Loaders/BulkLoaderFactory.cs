using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Loaders;

public sealed class BulkLoaderFactory(ILoggerFactory loggerFactory) : IBulkLoaderFactory
{
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
