using ETL.Core.Interfaces;
using ETL.Infrastructure.Audit;
using ETL.Infrastructure.DeadLetter;
using ETL.Infrastructure.Loaders;
using ETL.Infrastructure.Logging;
using ETL.Infrastructure.Processing;
using ETL.Infrastructure.SourceConfig;
using ETL.Infrastructure.UnitOfWork;
using ETL.Infrastructure.Watermarking;
using Microsoft.Extensions.DependencyInjection;

namespace ETL.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ETL engine services backed by SQLite.
    /// </summary>
    public static IServiceCollection AddEtlInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<IUnitOfWorkFactory>(_ => new SqliteUnitOfWorkFactory(connectionString));
        services.AddSingleton<IProcessLogger>(_ => new SqlProcessLogger(connectionString));
        services.AddSingleton<IWatermarkStore>(_ => new SqlWatermarkStore(connectionString));
        services.AddSingleton<IDeadLetterStore>(_ => new SqlDeadLetterStore(connectionString));
        services.AddSingleton<IAuditLog>(_ => new SqlAuditLog(connectionString));
        services.AddSingleton<IBulkLoaderFactory, BulkLoaderFactory>();
        services.AddSingleton<IProcessRunner, TransactionalProcessRunner>();
        return services;
    }

    /// <summary>
    /// Registers all ETL engine services including source config with encryption support.
    /// </summary>
    public static IServiceCollection AddEtlInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Func<string, string> encrypt,
        Func<string, string> decrypt)
    {
        services.AddEtlInfrastructure(connectionString);
        services.AddSingleton<ISourceConfigStore>(_ => new SqlSourceConfigStore(connectionString, encrypt, decrypt));
        return services;
    }
}
