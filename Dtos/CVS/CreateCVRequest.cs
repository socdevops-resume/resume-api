namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Request payload for creating a new CV.
/// </summary>
public class CreateCVRequest
{
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

    /// <summary>Contact email address.</summary>
    public required string Email { get; set; }

    /// <summary>Optional base64-encoded photo.</summary>
    public string? Photo { get; set; }

    /// <summary>Job title or professional headline.</summary>
    public required string JobTitle { get; set; }

    /// <summary>Professional summary or profile statement.</summary>
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

/// <summary>
/// A work experience entry for a CV.
/// </summary>
public class WorkExperienceDto
{
    /// <summary>Job position or role.</summary>
    public required string Position { get; set; }

    /// <summary>Company name.</summary>
    public required string Company { get; set; }

    /// <summary>Start date of the employment.</summary>
    public required DateTime StartDate { get; set; }

    /// <summary>End date of the employment (null if ongoing).</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Description of responsibilities and achievements.</summary>
    public required string Description { get; set; }
}

/// <summary>
/// An education entry for a CV.
/// </summary>
public class EducationDto
{
    /// <summary>Degree or qualification earned.</summary>
    public required string Degree { get; set; }

    /// <summary>Name of the school, college, or university.</summary>
    public required string School { get; set; }

    /// <summary>Start date of the study period.</summary>
    public required DateTime StartDate { get; set; }

    /// <summary>End date of the study period (null if ongoing).</summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// An external profile or portfolio link (e.g., LinkedIn, GitHub).
/// </summary>
public class LinkDto
{
    /// <summary>Type of link (e.g., "LinkedIn", "GitHub", "Website").</summary>
    public required string Type { get; set; }

    /// <summary>URL of the external resource.</summary>
    public required string Url { get; set; }
}
