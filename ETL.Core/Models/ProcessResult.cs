namespace ETL.Core.Models;

public sealed class ProcessResult
{
    public bool Success { get; init; }
    public int RecordsExtracted { get; init; }
    public int RecordsLoaded { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration { get; init; }

    public static ProcessResult Ok(int extracted, int loaded, TimeSpan duration) =>
        new() { Success = true, RecordsExtracted = extracted, RecordsLoaded = loaded, Duration = duration };

    public static ProcessResult Fail(string error, TimeSpan duration) =>
        new() { Success = false, ErrorMessage = error, Duration = duration };
}
