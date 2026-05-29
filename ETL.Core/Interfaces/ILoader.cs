namespace ETL.Core.Interfaces;

public interface ILoader<T>
{
    Task<int> LoadAsync(IEnumerable<T> records, CancellationToken cancellationToken = default);
}
