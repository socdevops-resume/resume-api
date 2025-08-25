using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace CVGeneratorAPI.Controllers;

/// <summary>
/// Session management endpoints: login and logout.
/// </summary>
/// <remarks>
/// Routes under <c>/api/sessions</c>.
/// </remarks>
[ApiController]
[Route("api/sessions")]
[Tags("Sessions")]
public class SessionsController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;
    private readonly ILogger<SessionsController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SessionsController"/>.
    /// </summary>
    /// <param name="userService">User lookup service.</param>
    /// <param name="tokenService">JWT issuing service.</param>
    public SessionsController(UserService userService, TokenService tokenService, ILogger<SessionsController> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and creates a session (login).
    /// </summary>
    /// <remarks>
    /// **Route:** <c>POST /api/sessions</c><br/>
    /// **Responses:**
    /// - <c>200 OK</c> with <see cref="AuthResponse"/> (JWT + user info) when credentials are valid.
    /// - <c>401 Unauthorized</c> when credentials are invalid.
    /// </remarks>
    /// <param name="request">Login credentials (username and password).</param>
    /// <returns>An <see cref="AuthResponse"/> containing the JWT and user information.</returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<AuthResponse>> Create([FromBody] LoginRequest request)
    {
        // Try username first, then email
        var identifier = request.Username?.Trim() ?? string.Empty;
        _logger.LogInformation("Login attempt for {Username}", request.Username);
        var user = await _userService.GetByUsernameAsync(identifier);
        user ??= await _userService.GetByEmailAsync(identifier);

        if (user is null)
        {
            return Unauthorized(new AuthResponse { Message = "Invalid credentials." });
        }

        // Compare against the same SHA-256 hashing scheme used in UsersController.
        if (user.PasswordHash != UserControllerHash(request.Password))
            return Unauthorized(new AuthResponse { Message = "Invalid credentials." });

        var token = _tokenService.Create(user);
        _logger.LogInformation("Login success for {Username}", user.Username);
        return Ok(new AuthResponse
        {
            Message = "Login successful.",
            Token = token,
            User = new UserResponse { Id = user.Id!, Username = user.Username, Email = user.Email }
        });
    }

    /// <summary>
    /// Ends the current session (logout).
    /// </summary>
    /// <remarks>
    /// **Route:** <c>DELETE /api/sessions</c><br/>
    /// JWT is stateless — clients should discard the token. This endpoint returns <c>204 No Content</c>.
    /// </remarks>
    /// <returns>No content.</returns>
    [Authorize]
    [HttpDelete]
    public IActionResult Delete() => NoContent();

    /// <summary>
    /// Computes a Base64-encoded SHA-256 hash of a password.
    /// </summary>
    /// <param name="password">The plaintext password.</param>
    /// <returns>Base64-encoded SHA-256 hash string.</returns>
    private static string UserControllerHash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
