namespace ETL.Core.Interfaces;

public interface IBulkLoaderFactory
{
    ILoader<T> CreateTruncateInsert<T>(IUnitOfWork uow, string table, string insertSql, int batchSize = 500);
    ILoader<T> CreateAppend<T>(IUnitOfWork uow, string insertSql, int batchSize = 500);
    ILoader<T> CreateUpsertDelete<T>(IUnitOfWork uow, string table, string keyColumn, Func<T, object> keySelector, string upsertSql, int batchSize = 500);
}
