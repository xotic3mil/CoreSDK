using ETL.Core.Interfaces;
using ETL.Core.Models;
using Microsoft.Extensions.Logging;

namespace ETL.Core;

public abstract class ProcessBase : IProcess
{
    protected readonly ILogger Logger;
    protected readonly IDeadLetterStore DeadLetterStore;

    protected ProcessBase(ILogger logger, IDeadLetterStore deadLetterStore)
    {
        Logger = logger;
        DeadLetterStore = deadLetterStore;
    }

    public virtual string Name => ProcessNameConvention.Derive(GetType());

    public abstract Task<ProcessResult> ExecuteAsync(IUnitOfWork uow, CancellationToken cancellationToken = default);
}
