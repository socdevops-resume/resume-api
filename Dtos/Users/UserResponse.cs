using System.ComponentModel.DataAnnotations;
using CVGeneratorAPI.Models;

namespace CVGeneratorAPI.Dtos;

/// <summary>Response payload representing a user (no sensitive data).</summary>
public record UserResponse(
    string Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string? Headline,
    string? Phone,
    string? Location,
    string? AvatarUrl,
    string? About,
    IReadOnlyList<Link> Links
);
