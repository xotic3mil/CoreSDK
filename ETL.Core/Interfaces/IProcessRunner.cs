using ETL.Core.Models;

namespace ETL.Core.Interfaces;

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(IProcess process, CancellationToken cancellationToken = default);
}
