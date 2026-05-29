namespace ETL.Core.Models;

public sealed record ProcessSchedule
{
    public string Name { get; init; } = "";
    public string? Cron { get; init; }
    public bool Enabled { get; init; }
    public string? Description { get; init; }
    public DateTime ModifiedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? CreatedAt { get; init; }
}
