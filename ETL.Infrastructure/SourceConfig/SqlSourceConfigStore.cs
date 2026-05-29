using System.Text.Json;
using Dapper;
using ETL.Core.Interfaces;
using ETL.Core.Models;
using Microsoft.Data.Sqlite;

namespace ETL.Infrastructure.SourceConfig;

public sealed class SqlSourceConfigStore(
    string connectionString,
    Func<string, string> encrypt,
    Func<string, string> decrypt) : ISourceConfigStore
{
    private const string SelectColumns = """
        name, description,
        base_url      AS BaseUrl,
        token_url     AS TokenUrl,
        client_id     AS ClientId,
        client_secret AS ClientSecret,
        headers       AS Headers,
        created_at    AS CreatedAt,
        updated_at    AS UpdatedAt
        """;

    public async Task<SourceConfig?> GetAsync(string name, CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        var row = await conn.QuerySingleOrDefaultAsync<SourceConfigRow>(
            $"SELECT {SelectColumns} FROM source_configs WHERE name = @name",
            new { name });
        return row is null ? null : ToModel(row);
    }

    public async Task<IReadOnlyList<SourceConfig>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        var rows = await conn.QueryAsync<SourceConfigRow>(
            $"SELECT {SelectColumns} FROM source_configs ORDER BY name");
        return rows.Select(ToModel).ToList();
    }

    public async Task SaveAsync(SourceConfig config, CancellationToken ct = default)
    {
        var encryptedSecret = config.ClientSecret is not null
            ? encrypt(config.ClientSecret)
            : null;

        var headersJson = config.Headers is { Count: > 0 }
            ? JsonSerializer.Serialize(config.Headers)
            : null;

        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        await conn.ExecuteAsync("""
            INSERT INTO source_configs
                (name, description, base_url, token_url, client_id, client_secret, headers, created_at, updated_at)
            VALUES
                (@Name, @Description, @BaseUrl, @TokenUrl, @ClientId, @ClientSecret, @Headers, @Now, @Now)
            ON CONFLICT (name) DO UPDATE SET
                description   = EXCLUDED.description,
                base_url      = EXCLUDED.base_url,
                token_url     = EXCLUDED.token_url,
                client_id     = EXCLUDED.client_id,
                client_secret = COALESCE(EXCLUDED.client_secret, source_configs.client_secret),
                headers       = EXCLUDED.headers,
                updated_at    = EXCLUDED.updated_at
            """, new
        {
            config.Name,
            config.Description,
            BaseUrl      = config.BaseUrl,
            config.TokenUrl,
            config.ClientId,
            ClientSecret = encryptedSecret,
            Headers      = headersJson,
            Now          = DateTime.UtcNow
        });
    }

    public async Task DeleteAsync(string name, CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM source_configs WHERE name = @name", new { name });
    }

    private SourceConfig ToModel(SourceConfigRow row) => new()
    {
        Name         = row.Name,
        Description  = row.Description,
        BaseUrl      = row.BaseUrl,
        TokenUrl     = row.TokenUrl,
        ClientId     = row.ClientId,
        ClientSecret = row.ClientSecret is not null ? TryDecrypt(row.ClientSecret) : null,
        Headers      = row.Headers is not null
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(row.Headers)
            : null,
        CreatedAt    = row.CreatedAt,
        UpdatedAt    = row.UpdatedAt
    };

    private string? TryDecrypt(string ciphertext)
    {
        try { return decrypt(ciphertext); }
        catch (System.Security.Cryptography.CryptographicException) { return null; }
    }

    private sealed record SourceConfigRow
    {
        public string Name { get; init; } = "";
        public string? Description { get; init; }
        public string BaseUrl { get; init; } = "";
        public string? TokenUrl { get; init; }
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
        public string? Headers { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
