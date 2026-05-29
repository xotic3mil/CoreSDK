namespace ETL.Core.Interfaces;

public interface IProcessLogger
{
    Task LogStartAsync(string processName, CancellationToken cancellationToken = default);
    Task LogCompleteAsync(string processName, int recordsProcessed, CancellationToken cancellationToken = default);
    Task LogErrorAsync(string processName, Exception ex, CancellationToken cancellationToken = default);
}
