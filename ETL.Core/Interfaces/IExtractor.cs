namespace ETL.Core.Interfaces;

public interface IExtractor<T>
{
    IAsyncEnumerable<T> ExtractAsync(CancellationToken cancellationToken = default);
}
