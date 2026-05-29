namespace ETL.Core.Models;

public sealed class WatermarkEntry
{
    public required string ProcessName { get; init; }
    public required string EntityName { get; init; }
    public DateTime LastRunAt { get; init; }
    public string? LastValue { get; init; }
}
