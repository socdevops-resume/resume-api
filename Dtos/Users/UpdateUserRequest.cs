using System.ComponentModel.DataAnnotations;
using CVGeneratorAPI.Models;

namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Request payload for updating the current user's profile.
/// All properties are optional; only non-null values are applied.
/// </summary>
public record UpdateUserRequest
{
    [MinLength(3), MaxLength(32)]
    public string? Username { get; init; }

    [EmailAddress, MaxLength(254)]
    public string? Email { get; init; }

    // Profile fields (optional)
    [MaxLength(50)]  public string? FirstName { get; init; }
    [MaxLength(50)]  public string? LastName  { get; init; }
    [MaxLength(80)]  public string? Headline  { get; init; }
    [Phone]          public string? Phone     { get; init; }
    [MaxLength(80)]  public string? Location  { get; init; }
    [Url]            public string? AvatarUrl { get; init; }
    [MaxLength(1000)]public string? About     { get; init; }

    // If provided, your controller/service should hash it and bump token version
    [MinLength(8), MaxLength(128)]
    public string? Password { get; init; }

    // Optional social links; replacing the entire set when not null.
    public List<Link>? Links { get; init; }
}
