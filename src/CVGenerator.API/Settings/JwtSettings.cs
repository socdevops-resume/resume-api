namespace CVGeneratorAPI.Settings;
/// <summary>
/// Configuration settings for JWT authentication.
/// This class is bound to the <c>Jwt</c> section in <c>appsettings.json</c>.
/// Used to configure token generation and validation.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// The issuer of the JWT token.
    /// Typically set to the API's domain or base URL.
    /// Used to validate the token's origin.
    /// </summary>
    public string Issuer { get; set; } = default!;

    /// <summary>
    /// The intended audience of the JWT token.
    /// Usually the client applications that consume the API.
    /// </summary>
    public string Audience { get; set; } = default!;

    /// <summary>
    /// The secret key used to sign the JWT token.
    /// Must be at least 32 characters when using HS256 algorithm.
    /// Keep this value secure and do not expose it publicly.
    /// </summary>
    public string Secret { get; set; } = default!;

    /// <summary>
    /// The expiration time of the JWT token in minutes.
    /// Default is 60 minutes.
    /// </summary>
    public int ExpMinutes { get; set; } = 60;
}