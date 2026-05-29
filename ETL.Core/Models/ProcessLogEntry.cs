namespace ETL.Core.Models;

public sealed class ProcessLogEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string ProcessName { get; init; }
    public ProcessStatus Status { get; set; }
    public int RecordsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public enum ProcessStatus { Running, Completed, Failed }
