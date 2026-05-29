namespace ETL.Core.Models;

public sealed class DeadLetterEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string ProcessName { get; init; }
    public required string Payload { get; init; }
    public required string ErrorMessage { get; init; }
    public DateTime FailedAt { get; init; } = DateTime.UtcNow;
}
