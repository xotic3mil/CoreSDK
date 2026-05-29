namespace ETL.Core.Models;

public sealed record SourceConfig
{
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public string BaseUrl { get; init; } = "";
    public string? TokenUrl { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public IReadOnlyDictionary<string, string>? Headers { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
