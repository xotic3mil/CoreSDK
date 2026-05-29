using ETL.Core.Models;

namespace ETL.Core.Interfaces;

public interface ISourceConfigStore
{
    Task<SourceConfig?> GetAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<SourceConfig>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(SourceConfig config, CancellationToken ct = default);
    Task DeleteAsync(string name, CancellationToken ct = default);
}
