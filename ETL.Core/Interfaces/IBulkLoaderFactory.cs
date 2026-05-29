namespace ETL.Core.Interfaces;

public interface IBulkLoaderFactory
{
    // Auto-SQL: table name, columns, and SQL derived from T via convention + attributes
    ILoader<T> CreateTruncateInsert<T>(IUnitOfWork uow, int batchSize = 500);
    ILoader<T> CreateAppend<T>(IUnitOfWork uow, int batchSize = 500);
    ILoader<T> CreateUpsertDelete<T>(IUnitOfWork uow, int batchSize = 500);

    // Manual-SQL: full control over table name and SQL
    ILoader<T> CreateTruncateInsert<T>(IUnitOfWork uow, string table, string insertSql, int batchSize = 500);
    ILoader<T> CreateAppend<T>(IUnitOfWork uow, string insertSql, int batchSize = 500);
    ILoader<T> CreateUpsertDelete<T>(IUnitOfWork uow, string table, string keyColumn, Func<T, object> keySelector, string upsertSql, int batchSize = 500);
}
