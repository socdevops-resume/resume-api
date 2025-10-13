using System.ComponentModel.DataAnnotations;

namespace CVGeneratorAPI.Dtos;

/// <summary>Request payload for creating a new CV.</summary>
public record CreateCVRequest
{
    [Required] public string FirstName { get; init; } = default!;
    [Required] public string LastName  { get; init; } = default!;
    [Required] public string City      { get; init; } = default!;
    [Required] public string Country   { get; init; } = default!;
    [Required] public string Postcode  { get; init; } = default!;
    [Required] public string Phone     { get; init; } = default!;
    [Required, EmailAddress] public string Email { get; init; } = default!;

    public string? Photo { get; init; }

    [Required] public string JobTitle { get; init; } = default!;
    [Required] public string Summary  { get; init; } = default!;

    [Required] public List<string> Skills { get; init; } = new();

    [Required] public List<WorkExperienceDto> WorkExperiences { get; init; } = new();
    [Required] public List<EducationDto>     Educations       { get; init; } = new();
    [Required] public List<LinkDto>          Links            { get; init; } = new();
}

public record WorkExperienceDto(
    [property: Required] string Position,
    [property: Required] string Company,
    [property: Required] DateTime StartDate,
    DateTime? EndDate,
    [property: Required] string Description
);

public record EducationDto(
    [property: Required] string Degree,
    [property: Required] string School,
    [property: Required] DateTime StartDate,
    DateTime? EndDate
);

public record LinkDto(
    [property: Required] string Type,
    [property: Required, Url] string Url
);
