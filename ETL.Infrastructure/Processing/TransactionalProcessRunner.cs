using ETL.Core.Interfaces;
using ETL.Core.Models;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Processing;

public sealed class TransactionalProcessRunner : IProcessRunner
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IProcessLogger _processLogger;
    private readonly ILogger<TransactionalProcessRunner> _logger;

    public TransactionalProcessRunner(
        IUnitOfWorkFactory uowFactory,
        IProcessLogger processLogger,
        ILogger<TransactionalProcessRunner> logger)
    {
        _uowFactory = uowFactory;
        _processLogger = processLogger;
        _logger = logger;
    }

    public async Task<ProcessResult> RunAsync(IProcess process, CancellationToken cancellationToken = default)
    {
        var started = DateTime.UtcNow;
        await _processLogger.LogStartAsync(process.Name, cancellationToken);

        await using var uow = await _uowFactory.CreateAsync(cancellationToken);
        try
        {
            var result = await process.ExecuteAsync(uow, cancellationToken);
            await uow.CommitAsync(cancellationToken);
            await _processLogger.LogCompleteAsync(process.Name, result.RecordsLoaded, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync(cancellationToken);
            await _processLogger.LogErrorAsync(process.Name, ex, cancellationToken);
            _logger.LogError(ex, "Process {Name} failed — transaction rolled back", process.Name);
            return ProcessResult.Fail(ex.Message, DateTime.UtcNow - started);
        }
    }
}
