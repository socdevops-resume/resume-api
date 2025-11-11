using System.Text.Json.Serialization;

namespace CVGeneratorAPI.Dtos;

public record ExperienceItem(
    [property: JsonPropertyName("company")] string Company,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("start")] string? Start,
    [property: JsonPropertyName("end")] string? End,
    [property: JsonPropertyName("bullets")] List<string> Bullets
);

public record EducationItem(
    [property: JsonPropertyName("school")] string School,
    [property: JsonPropertyName("degree")] string? Degree,
    [property: JsonPropertyName("year")] string? Year
);

public record CanonicalProfile(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("skills")] List<string> Skills,
    [property: JsonPropertyName("experience")] List<ExperienceItem> Experience,
    [property: JsonPropertyName("education")] List<EducationItem> Education
);
