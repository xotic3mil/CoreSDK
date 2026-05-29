using ETL.Core.Interfaces;
using ETL.Infrastructure.SqlGeneration;

namespace ETL.Infrastructure.Loaders;

public static class BulkLoaderFactoryExtensions
{
    public static ILoader<T> CreateTruncateInsert<T>(this IBulkLoaderFactory factory, IUnitOfWork uow, int batchSize = 500) =>
        factory.CreateTruncateInsert<T>(uow, SqlMapper<T>.TableName, SqlMapper<T>.InsertSql, batchSize);

    public static ILoader<T> CreateAppend<T>(this IBulkLoaderFactory factory, IUnitOfWork uow, int batchSize = 500) =>
        factory.CreateAppend<T>(uow, SqlMapper<T>.InsertSql, batchSize);

    public static ILoader<T> CreateUpsertDelete<T>(this IBulkLoaderFactory factory, IUnitOfWork uow, int batchSize = 500)
    {
        if (string.IsNullOrEmpty(SqlMapper<T>.KeyColumn))
            throw new InvalidOperationException(
                $"Cannot generate UPSERT SQL for {typeof(T).Name}: no [Key] attribute found. " +
                "Annotate the key property with [Key] or use the manual overload.");

        return factory.CreateUpsertDelete<T>(uow, SqlMapper<T>.TableName, SqlMapper<T>.KeyColumn, SqlMapper<T>.KeySelector, SqlMapper<T>.UpsertSql, batchSize);
    }
}
