using System.Text.Json.Serialization;

namespace CVGeneratorAPI.Dtos;

public record JDRequest(
    [property: JsonPropertyName("job_description")] string JobDescription
);

public record KeywordsResponse(
    [property: JsonPropertyName("skills")] List<string> Skills,
    [property: JsonPropertyName("keywords")] List<string> Keywords,
    [property: JsonPropertyName("seniority")] string? Seniority,
    [property: JsonPropertyName("nice_to_have")] List<string> NiceToHave
);