// Controllers/UsersController.cs
using System.Security.Claims;
using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Models;
using CVGeneratorAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVGeneratorAPI.Controllers;

/// <summary>
/// User account management endpoints (signup, read/update/delete self).
/// </summary>
/// <remarks>Base route: <c>/api/users</c>.</remarks>
[ApiController]
[Route("api/users")]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly UserService _users;
    private readonly TokenService _tokens;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserService users,
        TokenService tokens,
        IPasswordHasher hasher,
        ILogger<UsersController> logger)
    {
        _users = users;
        _tokens = tokens;
        _hasher = hasher;
        _logger = logger;
    }

    // ---------------------------
    // Helpers
    // ---------------------------

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

    private string? GetAuthUserId() =>
        User.FindFirst("sub")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    // ---------------------------
    // POST /api/users  (signup)
    // ---------------------------

    /// <summary>Registers a new user account.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AuthResponse>> Create([FromBody] SignUpRequest request)
    {
        _logger.LogInformation("Signup attempt for {Username} {Email}", request.Username, request.Email);

        // Uniqueness checks
        if (await _users.GetByUsernameAsync(request.Username) is not null)
            return BadRequest(new AuthResponse { Message = "Username already exists." });

        if (await _users.GetByEmailAsync(request.Email) is not null)
            return BadRequest(new AuthResponse { Message = "Email already exists." });

        var user = new UserModel
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password),
            // Roles, etc., defaulted by model
        };

        await _users.CreateUserAsync(user);

        // Issue JWT
        var token = _tokens.Create(user);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new AuthResponse
        {
            Message = "User created successfully.",
            Token = token,
            User = ToUserResponse(user)
        });
    }

    // ---------------------------
    // GET /api/users/{id}
    // ---------------------------

    /// <summary>Retrieves the current authenticated user's details.</summary>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserResponse>> GetById(string id)
    {
        var currentUserId = GetAuthUserId();
        if (currentUserId != id)
        {
            _logger.LogWarning("Forbidden GET /api/users/{Id}: AuthUserId={AuthUserId}", id, currentUserId);
            return Forbid();
        }

        var user = await _users.GetByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("User not found on GET: UserId={Id}", id);
            return NotFound("User not found.");
        }

        _logger.LogInformation("User fetched: UserId={Id}, Username={Username}", user.Id, user.Username);
        return Ok(ToUserResponse(user));
    }

    // ---------------------------
    // PUT /api/users/{id}
    // (PATCH-like semantics: only non-null props are applied)
    // ---------------------------

    /// <summary>Updates the authenticated user's details (partial update).</summary>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserResponse>> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = GetAuthUserId();
        if (currentUserId != id)
        {
            _logger.LogWarning("Forbidden UPDATE /api/users/{Id}: AuthUserId={AuthUserId}", id, currentUserId);
            return Forbid();
        }

        var existingUser = await _users.GetByIdAsync(id);
        if (existingUser is null)
        {
            _logger.LogWarning("User not found on update: UserId={Id}", id);
            return NotFound("User not found.");
        }

        // Uniqueness checks only if fields provided (non-null)
        if (request.Username is not null)
        {
            var byUsername = await _users.GetByUsernameAsync(request.Username);
            if (byUsername is not null && byUsername.Id != id)
                return BadRequest("Username already taken.");
        }
        if (request.Email is not null)
        {
            var byEmail = await _users.GetByEmailAsync(request.Email);
            if (byEmail is not null && byEmail.Id != id)
                return BadRequest("Email already in use.");
        }

        // Apply profile updates atomically (only provided fields)
        var updated = await _users.UpdateProfileAsync(id, u => u
            .SetIfNotNull(request.Username,  x => x.Username)
            .SetIfNotNull(request.Email,     x => x.Email)
            .SetIfNotNull(request.FirstName, x => x.FirstName)
            .SetIfNotNull(request.LastName,  x => x.LastName)
            .SetIfNotNull(request.Headline,  x => x.Headline)
            .SetIfNotNull(request.Phone,     x => x.Phone)
            .SetIfNotNull(request.Location,  x => x.Location)
            .SetIfNotNull(request.AvatarUrl, x => x.AvatarUrl)
            .SetIfNotNull(request.About,     x => x.About)
            .ReplaceListIfProvided(request.Links, x => x.Links)
        );

        // Optional password change in the same request
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            await _users.UpdatePasswordAsync(id, request.Password, bumpTokenVersion: true);
            // NOTE: This invalidates existing JWTs. Frontend should prompt re-login.
        }

        _logger.LogInformation("User updated: UserId={Id}, Username={Username}", id, updated.Username);
        return Ok(ToUserResponse(updated));
    }

    // ---------------------------
    // DELETE /api/users/{id}
    // ---------------------------

    /// <summary>Deletes the authenticated user's account.</summary>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    public async Task<ActionResult> Delete(string id)
    {
        var currentUserId = GetAuthUserId();
        if (currentUserId != id)
        {
            _logger.LogWarning("Forbidden DELETE /api/users/{Id}: AuthUserId={AuthUserId}", id, currentUserId);
            return Forbid();
        }

        await _users.DeleteUserAsync(id);
        _logger.LogInformation("User deleted: UserId={Id}", id);
        return NoContent();
    }
}
