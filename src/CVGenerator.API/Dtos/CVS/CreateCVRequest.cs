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
    [Required] public List<EducationDto> Educations { get; init; } = new();
    [Required] public List<LinkDto> Links { get; init; } = new();
}

public record WorkExperienceDto
{
    [Required] public string Position { get; init; } = default!;
    [Required] public string Company { get; init; } = default!;
    [Required] public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    [Required] public string Description { get; init; } = default!;
}

public record EducationDto
{
    [Required] public string Degree { get; init; } = default!;
    [Required] public string School { get; init; } = default!;
    [Required] public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public record LinkDto
{
    [Required] public string Type { get; init; } = default!;
    [Required, Url] public string Url { get; init; } = default!;
}