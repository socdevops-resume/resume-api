namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Response payload representing a CV (Curriculum Vitae).
/// </summary>
public class CVResponse
{
    /// <summary>
    /// Unique identifier of the CV.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>First name of the CV owner.</summary>
    public required string FirstName { get; set; }

    /// <summary>Last name of the CV owner.</summary>
    public required string LastName { get; set; }

    /// <summary>City of residence.</summary>
    public required string City { get; set; }

    /// <summary>Country of residence.</summary>
    public required string Country { get; set; }

    /// <summary>Postal/ZIP code.</summary>
    public required string Postcode { get; set; }

    /// <summary>Contact phone number.</summary>
    public required string Phone { get; set; }

    /// <summary>Email address of the CV owner.</summary>
    public required string Email { get; set; }

    /// <summary>Optional base64-encoded profile photo.</summary>
    public string? Photo { get; set; }

    /// <summary>Current or desired job title.</summary>
    public required string JobTitle { get; set; }

    /// <summary>Professional summary or headline.</summary>
    public required string Summary { get; set; }

    /// <summary>List of skills.</summary>
    public required List<string> Skills { get; set; } = new();

    /// <summary>List of work experiences.</summary>
    public required List<WorkExperienceDto> WorkExperiences { get; set; } = new();

    /// <summary>List of educational qualifications.</summary>
    public required List<EducationDto> Educations { get; set; } = new();

    /// <summary>List of external links (e.g., LinkedIn, GitHub, Website).</summary>
    public required List<LinkDto> Links { get; set; } = new();
}
