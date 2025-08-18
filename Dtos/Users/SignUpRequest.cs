namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Request payload for registering a new user account.
/// </summary>
public class SignUpRequest
{
    /// <summary>
    /// Desired username for the new account.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Email address of the new user.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Plain-text password to secure the account.
    /// </summary>
    /// <remarks>
    /// This will be hashed before storing in the database.
    /// </remarks>
    public required string Password { get; set; }
}
