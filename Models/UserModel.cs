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
    public string[] Roles { get; set; } = new[] { "User" };

    /// <summary>Timestamp of when the user was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
     /// <summary>Timestamp of the last update to the user.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
