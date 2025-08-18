namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Request payload for updating an existing user account.
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// The updated username.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The updated email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// New password to replace the old one (optional).
    /// </summary>
    /// <remarks>
    /// If not provided, the existing password will remain unchanged.
    /// </remarks>
    public string? Password { get; set; }
}
