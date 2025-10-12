using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CVGeneratorAPI.Models;

/// <summary>
/// Represents a CV (Curriculum Vitae) belonging to a specific user.
/// </summary>
public class CVModel
{
    /// <summary>
    /// MongoDB document identifier.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Identifier of the user who owns this CV.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; }

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

    /// <summary>Phone number of the CV owner.</summary>
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
    public required List<WorkExperience> WorkExperiences { get; set; } = new();

    /// <summary>List of education entries.</summary>
    public required List<Education> Educations { get; set; } = new();

    /// <summary>List of external links (e.g., LinkedIn, GitHub, Website).</summary>
    public required List<Link> Links { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// A work experience entry inside a CV.
/// </summary>
public class WorkExperience
{
    /// <summary>Job position or role.</summary>
    public required string Position { get; set; }

    /// <summary>Company name.</summary>
    public required string Company { get; set; }

    /// <summary>Start date of the role.</summary>
    public required DateTime StartDate { get; set; }

    /// <summary>Optional end date (null if ongoing).</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Description of responsibilities and achievements.</summary>
    public required string Description { get; set; }
}

/// <summary>
/// An education entry inside a CV.
/// </summary>
public class Education
{
    /// <summary>Degree or qualification earned.</summary>
    public required string Degree { get; set; }

    /// <summary>School, college, or university name.</summary>
    public required string School { get; set; }

    /// <summary>Start date of the study program.</summary>
    public required DateTime StartDate { get; set; }

    /// <summary>Optional end date (null if ongoing).</summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// An external profile or portfolio link (e.g., LinkedIn, GitHub).
/// </summary>
public class Link
{
    /// <summary>
    /// Type of link (e.g., "LinkedIn", "GitHub", "Website").
    /// </summary>
    public required string Type { get; set; }

    /// <summary>URL of the external link.</summary>
    public required string Url { get; set; }
}
