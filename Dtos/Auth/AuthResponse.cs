namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Standard response returned after authentication (signup or login).
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Human-readable message about the authentication result.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// The JWT token issued if authentication was successful.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// The authenticated user's details, or <c>null</c> if not applicable.
    /// </summary>
    public UserResponse? User { get; set; }
}
