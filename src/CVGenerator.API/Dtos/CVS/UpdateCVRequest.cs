using System.ComponentModel.DataAnnotations;

namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Partial update payload. Only non-null properties are applied.
/// Lists: if provided, they REPLACE the stored lists; if null, leave as-is.
/// </summary>
public record UpdateCVRequest
{
    public string? FirstName { get; init; }
    public string? LastName  { get; init; }
    public string? City      { get; init; }
    public string? Country   { get; init; }
    public string? Postcode  { get; init; }
    public string? Phone     { get; init; }
    [EmailAddress] public string? Email { get; init; }

    public string? Photo { get; init; }
    public string? JobTitle { get; init; }
    public string? Summary  { get; init; }

    public List<string>?            Skills           { get; init; }
    public List<WorkExperienceDto>? WorkExperiences  { get; init; }
    public List<EducationDto>?      Educations       { get; init; }
    public List<LinkDto>?           Links            { get; init; }
}
