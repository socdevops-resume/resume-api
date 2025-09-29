// Controllers/UsersController.cs
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Models;
using CVGeneratorAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVGeneratorAPI.Controllers;

/// <summary>
/// User account management endpoints (signup, read/update/delete self, and admin role management).
/// </summary>
/// <remarks>
/// Base route: <c>/api/users</c>.
/// </remarks>
[ApiController]
[Route("api/users")]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="UsersController"/>.
    /// </summary>
    /// <param name="userService">Service for user persistence.</param>
    /// <param name="tokenService">Service for issuing JWT tokens.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="hasher">Password hashing service.</param>
    public UsersController(
        UserService userService,
        TokenService tokenService,
        IPasswordHasher hasher,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _hasher = hasher;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <remarks>
    /// **Route:** <c>POST /api/users</c><br/>
    /// **Responses:**
    /// - <c>201 Created</c> with an <see cref="AuthResponse"/> containing the JWT and user info.
    /// - <c>400 Bad Request</c> if the username already exists.
    /// </remarks>
    /// <param name="request">Signup details including username, email, and password.</param>
    /// <returns>An <see cref="AuthResponse"/> with JWT token and user data when successful.</returns>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AuthResponse>> Create([FromBody] SignUpRequest request)
    {
        var existing = await _userService.GetByUsernameAsync(request.Username);
        _logger.LogInformation("Signup attempt for {Username} {Email}", request.Username, request.Email);

        if (existing != null)
            return BadRequest(new AuthResponse { Message = "Username already exists." });

        var user = new UserModel
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password),
        };

        await _userService.CreateUserAsync(user);

        // Issue JWT for the new user
        var token = _tokenService.Create(user);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new AuthResponse
        {
            Message = "User created successfully.",
            Token = token,
            User = new UserResponse { Id = user.Id!, Username = user.Username, Email = user.Email }
        });
    }

    /// <summary>
    /// Retrieves the current authenticated user's details by ID.
    /// </summary>
    /// <remarks>
    /// **Route:** <c>GET /api/users/{id}</c><br/>
    /// **Auth:** Requires a valid JWT and the <c>{id}</c> must match the token's subject.<br/>
    /// **Responses:**
    /// - <c>200 OK</c> with <see cref="UserResponse"/>.
    /// - <c>403 Forbid</c> if <c>{id}</c> does not match the authenticated user.
    /// - <c>404 Not Found</c> if the user record does not exist.
    /// </remarks>
    /// <param name="id">The user ID (must match the authenticated user's ID).</param>
    /// <returns>The user's public profile data.</returns>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserResponse>> GetById(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != id)
        {
            _logger.LogWarning("Forbidden GET /api/users/{Id}: AuthUserId={AuthUserId}", id, currentUserId);
            return Forbid();
        }

        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User not found on GET: UserId={Id}", id);
            return NotFound("User not found.");
        }

        _logger.LogInformation("User fetched: UserId={Id}, Username={Username}", user.Id, user.Username);
        return Ok(new UserResponse { Id = user.Id!, Username = user.Username, Email = user.Email });
    }

    /// <summary>
    /// Updates the authenticated user's details.
    /// </summary>
    /// <remarks>
    /// **Route:** <c>PUT /api/users/{id}</c><br/>
    /// **Auth:** Requires a valid JWT and the <c>{id}</c> must match the token's subject.<br/>
    /// **Responses:**
    /// - <c>200 OK</c> with the updated <see cref="UserResponse"/>.
    /// - <c>400 Bad Request</c> if the new username is already taken by another user.
    /// - <c>403 Forbid</c> if <c>{id}</c> does not match the authenticated user.
    /// - <c>404 Not Found</c> if the user record does not exist.
    /// </remarks>
    /// <param name="id">The user ID (must match the authenticated user's ID).</param>
    /// <param name="request">Updated user details (username, email, password).</param>
    /// <returns>The updated user data.</returns>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserResponse>> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != id)
        {
            _logger.LogWarning("Forbidden UPDATE /api/users/{Id}: AuthUserId={AuthUserId}", id, currentUserId);
            return Forbid();
        }

        var existingByUsername = await _userService.GetByUsernameAsync(request.Username);
        if (existingByUsername != null && existingByUsername.Id != id)
        {
            _logger.LogWarning("Username collision on update: Requested={Requested}, OwnerId={OwnerId}, CurrentUserId={UserId}",
                request.Username, existingByUsername.Id, id);
            return BadRequest("Username already taken.");
        }

        var existingUser = await _userService.GetByIdAsync(id);
        if (existingUser == null)
        {
            _logger.LogWarning("User not found on update: UserId={Id}", id);
            return NotFound("User not found.");
        }

        var passwordHash = string.IsNullOrWhiteSpace(request.Password)
            ? existingUser.PasswordHash
            : _hasher.Hash(request.Password); ;

        var updated = new UserModel
        {
            Id = id,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Roles = existingUser.Roles // preserve roles on self-update
        };

        await _userService.UpdateUserAsync(id, updated);
        _logger.LogInformation("User updated: UserId={Id}, Username={Username}", id, updated.Username);

        return Ok(new UserResponse { Id = updated.Id!, Username = updated.Username, Email = updated.Email });
    }

    /// <summary>
    /// Deletes the authenticated user's account.
    /// </summary>
    /// <remarks>
    /// **Route:** <c>DELETE /api/users/{id}</c><br/>
    /// **Auth:** Requires a valid JWT and the <c>{id}</c> must match the token's subject.<br/>
    /// **Responses:**
    /// - <c>204 No Content</c> on successful deletion.
    /// - <c>403 Forbid</c> if <c>{id}</c> does not match the authenticated user.
    /// </remarks>
    /// <param name="id">The user ID (must match the authenticated user's ID).</param>
    /// <returns>No content when deletion succeeds.</returns>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> Delete(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != id)
        {
            _logger.LogWarning("Forbidden DELETE /api/users/{Id}: AuthUserId={AuthUserId}", id, currentUserId);
            return Forbid();
        }

        await _userService.DeleteUserAsync(id);
        _logger.LogInformation("User deleted: UserId={Id}", id);

        return NoContent();
    }
}
