using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CVGeneratorAPI.Models;


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
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Initializes a new instance of <see cref="SessionsController"/>.
    /// </summary>
    /// <param name="userService">User lookup service.</param>
    /// <param name="tokenService">JWT issuing service.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="passwordHasher">Password hashing service.</param>
    public SessionsController(UserService userService, TokenService tokenService, ILogger<SessionsController> logger, IPasswordHasher passwordHasher)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    
    private static UserResponse ToUserResponse(UserModel u) => new(
        u.Id!,
        u.Username,
        u.Email,
        u.FirstName,
        u.LastName,
        u.Headline,
        u.Phone,
        u.Location,
        u.AvatarUrl,
        u.About,
        u.Links ?? new List<Link>()
    );

    /// <summary>
    /// Authenticates a user and creates a session (login).
    /// </summary>
    /// <remarks>
    /// Route: POST /api/sessions
    /// </remarks>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(AuthResponse), 401)]
    public async Task<ActionResult<AuthResponse>> Create([FromBody] LoginRequest request)
    {
        var identifier = (request.Username ?? string.Empty).Trim();
        _logger.LogInformation("Login attempt for identifier: {Identifier}", identifier);

        // Try username first, then email
        var user = await _userService.GetByUsernameAsync(identifier)
                ?? await _userService.GetByEmailAsync(identifier);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for identifier {Identifier}", identifier);
            return Unauthorized(new AuthResponse { Message = "Invalid credentials." });
        }

        var passwordOk = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordOk)
        {
            // IMPORTANT: never log the password; log only the identifier.
            _logger.LogWarning("Login failed: invalid password for {Identifier}", identifier);
            return Unauthorized(new AuthResponse { Message = "Invalid credentials." });
        }

        var token = _tokenService.Create(user);
        _logger.LogInformation("Login success for {Username}", user.Username);

        return Ok(new AuthResponse
        {
            Message = "Login successful.",
            Token = token,
            User = ToUserResponse(user)
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

   
}
