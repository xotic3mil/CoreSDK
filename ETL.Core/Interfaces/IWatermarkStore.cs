using ETL.Core.Models;

namespace ETL.Core.Interfaces;

public interface IWatermarkStore
{
    Task<WatermarkEntry?> GetAsync(string processName, string entityName, CancellationToken cancellationToken = default);
    Task SetAsync(WatermarkEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WatermarkEntry>> ListAllAsync(CancellationToken cancellationToken = default);
    Task DeleteByProcessAsync(string processName, CancellationToken cancellationToken = default);
}
