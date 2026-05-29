using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ETL.Infrastructure.Http;

public sealed class OAuthTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _tokenEndpoint;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _scope;

    private string? _cachedToken;
    private DateTime _expiresAt = DateTime.MinValue;

    public OAuthTokenProvider(HttpClient httpClient, string tokenEndpoint,
        string clientId, string clientSecret, string scope = "")
    {
        _httpClient = httpClient;
        _tokenEndpoint = tokenEndpoint;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _scope = scope;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _expiresAt)
            return _cachedToken;

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["scope"] = _scope
        };

        var response = await _httpClient.PostAsync(_tokenEndpoint, new FormUrlEncodedContent(form), cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("OAuth token response was empty");

        _cachedToken = token.AccessToken;
        _expiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn - 30);
        return _cachedToken;
    }

    private sealed class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; } = "";
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
    }
}
