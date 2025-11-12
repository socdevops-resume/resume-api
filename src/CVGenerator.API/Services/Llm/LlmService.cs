using CVGeneratorAPI.Dtos;

namespace CVGeneratorAPI.Services.Llm;

public class LlmService
{
    private readonly ILlmClient _client;
    public LlmService(ILlmClient client) => _client = client;

    public Task<KeywordsResponse?> ExtractKeywordsAsync(JDRequest req, CancellationToken ct = default) =>
        _client.PostAsync<KeywordsResponse>("/resume/keywords", req, ct);

    public Task<SummaryResponse?> GetSummaryAsync(SummaryRequest req, CancellationToken ct = default) =>
        _client.PostAsync<SummaryResponse>("/resume/summary", req, ct);

    public Task<CoverLetterResponse?> GetCoverLetterAsync(CoverLetterRequest req, CancellationToken ct = default) =>
        _client.PostAsync<CoverLetterResponse>("/resume/cover-letter", req, ct);
}