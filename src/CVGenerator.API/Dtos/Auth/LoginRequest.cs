namespace CVGeneratorAPI.Dtos;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request payload used when logging into the system.
/// </summary>
public record LoginRequest(
    [Required] string Username,      // can be username OR email
    [Required] string Password
);

