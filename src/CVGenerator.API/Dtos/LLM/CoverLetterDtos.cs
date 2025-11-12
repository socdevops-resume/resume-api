using System.Text.Json.Serialization;

namespace CVGeneratorAPI.Dtos;

public record CoverLetterRequest(
    [property: JsonPropertyName("profile")] CanonicalProfile Profile,
    [property: JsonPropertyName("job_description")] string JobDescription,
    [property: JsonPropertyName("company")] string? Company,
    [property: JsonPropertyName("role")] string? Role
);

public record CoverLetterResponse(
    [property: JsonPropertyName("cover_letter")] string CoverLetter
);
