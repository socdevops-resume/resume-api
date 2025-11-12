using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CVGeneratorAPI.Models;

/// <summary>
/// Enum representing the template engine types.        
/// </summary>
public enum TemplateEngine { ReactSchema, Markup }

/// <summary>
/// Represents a CV (Curriculum Vitae) templates that are used to format te document.        
/// </summary>
/// <remarks>
/// This model includes properties for the template's name, description, and file path. 
/// It is used to manage and retrieve CV templates within the application.
/// </remarks>
public class CVTemplateModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required string Name { get; set; }
    public required string Description { get; set; }
    public TemplateEngine Engine { get; set; }

    // --- Markup engine fields ---
    public string? Markup { get; set; }      // HTML with {{placeholders}}
    public string? Css { get; set; }         // optional CSS text

    // --- React-schema engine fields ---
    public BsonDocument? Tokens { get; set; } // { title: "text-2xl ..." }
    public BsonArray? Layout { get; set; }    // JSON layout DSL

    // Shared metadata
    public string Version { get; set; } = "1.0.0";
    public string[] Variables { get; set; } = Array.Empty<string>();
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? PreviewImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}