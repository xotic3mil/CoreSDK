using ETL.Core.Models;

namespace ETL.Core.Interfaces;

public interface IProcessScheduleStore
{
    Task EnsureRegisteredAsync(string name, string? createdBy = null, CancellationToken ct = default);
    Task<ProcessSchedule?> GetAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<ProcessSchedule>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(string name, string? cron, bool enabled, string? description = null, CancellationToken ct = default);
}
