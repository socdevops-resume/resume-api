
namespace CVGeneratorAPI.Settings;
public sealed class LlmSettings
{
    public string BaseUrl { get; set; } = default!;
    public string ApiKeyHeader { get; set; } = "X-API-Key";
    public string ApiKey { get; set; } = default!;
}