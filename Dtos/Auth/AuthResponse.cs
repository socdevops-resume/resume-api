namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Standard response returned after authentication (signup or login).
/// </summary>
public record AuthResponse
{
    public string? Message { get; init; }
    public string? Token { get; init; }
    public UserResponse? User { get; init; }
}
