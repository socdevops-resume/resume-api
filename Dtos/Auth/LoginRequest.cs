namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Request payload used when logging into the system.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username of the account attempting to log in.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The plain-text password provided for authentication.
    /// </summary>
    /// <remarks>
    /// This password will be hashed and validated against the stored password hash.
    /// </remarks>
    public required string Password { get; set; }
}
