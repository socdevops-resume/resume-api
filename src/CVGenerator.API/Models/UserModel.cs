using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CVGeneratorAPI.Models;

/// <summary>
/// Represents an application user with authentication details.
/// </summary>
public class UserModel
{
    /// <summary>MongoDB document identifier.</summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>Unique username chosen by the user.</summary>
    public required string Username { get; set; }

    /// <summary>User’s email address.</summary>
    public required string Email { get; set; }

    /// <summary>
    /// SHA-256 hash of the user’s password.
    /// Stored securely instead of plaintext.
    /// </summary>
    public required string PasswordHash { get; set; }

    // Simple role list. Default is a regular user.
    /// <summary>
    /// List of roles assigned to the user (e.g., "User", "Admin"). 
    /// Default is ["User"].
    /// </summary>
    public string[] Roles { get; set; } = ["User"];

    /// <Summary>
    /// Profile details.
    /// These fields can be updated by the user.
    /// They are optional during registration.
    /// </Summary>
    public string? FirstName { get; set; }
    public string? LastName  { get; set; }
    public string? Headline  { get; set; }          // e.g., "Frontend Developer"
    public string? Phone     { get; set; }
    public string? Location  { get; set; }
    public string? AvatarUrl { get; set; }          // stored URL (S3/Blob/local)
    public string? About { get; set; }          // “bio/summary”
    public int TokenVersion { get; set; } = 1;
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiresAt { get; set; }
    /// <summary>
    /// List of links associated with the user profile. 
    /// E.g., personal website, GitHub, LinkedIn.
    /// </summary>
    public List<Link> Links { get; set; } = [];  // website, github, linkedin

    /// <summary>Timestamp of when the user was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
     /// <summary>Timestamp of the last update to the user.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


