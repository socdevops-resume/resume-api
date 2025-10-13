namespace CVGeneratorAPI.Dtos;

/// <summary>Response payload representing a CV.</summary>
public record CVResponse(
    string Id,
    string FirstName,
    string LastName,
    string City,
    string Country,
    string Postcode,
    string Phone,
    string Email,
    string? Photo,
    string JobTitle,
    string Summary,
    List<string> Skills,
    List<WorkExperienceDto> WorkExperiences,
    List<EducationDto> Educations,
    List<LinkDto> Links
);
