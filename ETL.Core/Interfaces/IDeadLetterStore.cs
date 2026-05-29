namespace ETL.Core.Interfaces;

public interface IDeadLetterStore
{
    Task WriteAsync<T>(string processName, T record, Exception ex, CancellationToken cancellationToken = default);
}
