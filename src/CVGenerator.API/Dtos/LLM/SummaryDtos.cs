using System.Text.Json.Serialization;

namespace CVGeneratorAPI.Dtos;

public record SummaryRequest(
    [property: JsonPropertyName("profile")] CanonicalProfile Profile,
    [property: JsonPropertyName("job_description")] string? JobDescription
);

public record SummaryResponse(
    [property: JsonPropertyName("summary")] string Summary
);
