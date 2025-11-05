using System.Net.Http;
using System.Net.Http.Json;

public class LlmClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<LlmClient> _logger;

    public LlmClient(HttpClient http, IConfiguration cfg, ILogger<LlmClient> logger)
    {
        _http = http;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task<T?> PostAsync<T>(string path, object body, CancellationToken ct = default)
    {
        var headerName = _cfg["Llm:ApiKeyHeader"] ?? "X-API-Key";
        var apiKey     = _cfg["Llm:ApiKey"] ?? throw new InvalidOperationException("Llm:ApiKey not set");

        using var req = new HttpRequestMessage(HttpMethod.Post, path)
        { Content = JsonContent.Create(body) };

        req.Headers.TryAddWithoutValidation(headerName, apiKey);

        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }
}
