namespace ETL.Core.Interfaces;

public interface IAuditLog
{
    Task LogAsync(string? processName, string action, string? detail = null, CancellationToken ct = default);
}
