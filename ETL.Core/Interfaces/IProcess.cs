using ETL.Core.Models;

namespace ETL.Core.Interfaces;

public interface IProcess
{
    string Name { get; }
    Task<ProcessResult> ExecuteAsync(IUnitOfWork uow, CancellationToken cancellationToken = default);
}
