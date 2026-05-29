using System.Data;
using System.Runtime.CompilerServices;
using Dapper;
using ETL.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETL.Infrastructure.Extractors;

public sealed class SqlExtractor<T> : IExtractor<T>
{
    private readonly IDbConnection _connection;
    private readonly string _sql;
    private readonly object? _parameters;
    private readonly ILogger<SqlExtractor<T>>? _logger;

    public SqlExtractor(IDbConnection connection, string sql, object? parameters = null,
        ILogger<SqlExtractor<T>>? logger = null)
    {
        _connection = connection;
        _sql = sql;
        _parameters = parameters;
        _logger = logger;
    }

    public async IAsyncEnumerable<T> ExtractAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Executing SQL extract: {Sql}", _sql);
        var results = await _connection.QueryAsync<T>(new CommandDefinition(_sql, _parameters, cancellationToken: cancellationToken));
        foreach (var item in results)
            yield return item;
    }
}
