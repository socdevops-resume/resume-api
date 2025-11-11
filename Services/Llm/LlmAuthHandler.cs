using CVGeneratorAPI.Settings;
using Microsoft.Extensions.Options;

namespace CVGeneratorAPI.Services.Llm;

public sealed class LlmAuthHandler : DelegatingHandler
{
    private readonly IOptions<LlmSettings> _opt;
    public LlmAuthHandler(IOptions<LlmSettings> opt) => _opt = opt;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var s = _opt.Value;
        if (!request.Headers.Contains(s.ApiKeyHeader))
            request.Headers.TryAddWithoutValidation(s.ApiKeyHeader, s.ApiKey);

        return base.SendAsync(request, ct);
    }
}