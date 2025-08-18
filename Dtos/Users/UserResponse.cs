namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Response payload representing a user account (without sensitive data).
/// </summary>
public class UserResponse
{
    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Username of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Email address of the user.
    /// </summary>
    public required string Email { get; set; }
}
