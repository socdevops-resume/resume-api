using System.ComponentModel.DataAnnotations;

namespace CVGeneratorAPI.Dtos;

/// <summary>Request payload for registering a new user account.</summary>
public record SignUpRequest
{
    /// <summary>Desired username for the new account.</summary>
    [Required, MinLength(3), MaxLength(32)]
    public string Username { get; init; } = default!;

    /// <summary>Email address of the new user.</summary>
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; init; } = default!;

    /// <summary>Plain-text password (will be hashed by the server).</summary>
    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; init; } = default!;
}
