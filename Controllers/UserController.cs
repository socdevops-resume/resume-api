using CVGeneratorAPI.Models;
using CVGeneratorAPI.Services;
using CVGeneratorAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CVGeneratorAPI.Controllers;

/// <summary>
/// User account management endpoints (signup, read/update self, delete self).
/// </summary>
/// <remarks>
/// All endpoints under <c>/api/users</c>.
/// </remarks>
[ApiController]
[Route("api/users")]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly TokenService _tokenService;

    /// <summary>
    /// Initializes a new instance of <see cref="UsersController"/>.
    /// </summary>
    /// <param name="userService">Service for user persistence.</param>
    /// <param name="tokenService">Service for issuing JWT tokens.</param>
    public UsersController(UserService userService, TokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
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
    public async Task<ActionResult<AuthResponse>> Create([FromBody] SignUpRequest request)
    {
        var existing = await _userService.GetByUsernameAsync(request.Username);
        if (existing != null)
            return BadRequest(new AuthResponse { Message = "Username already exists." });

        var user = new UserModel
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password)
        };
        await _userService.CreateUserAsync(user);

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
    public async Task<ActionResult<UserResponse>> GetById(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != id) return Forbid();

        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound("User not found.");

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
    public async Task<ActionResult<UserResponse>> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != id) return Forbid();

        var existingByUsername = await _userService.GetByUsernameAsync(request.Username);
        if (existingByUsername != null && existingByUsername.Id != id)
            return BadRequest("Username already taken.");

        var existingUser = await _userService.GetByIdAsync(id);
        if (existingUser == null) return NotFound("User not found.");

        var passwordHash = string.IsNullOrWhiteSpace(request.Password)
            ? existingUser.PasswordHash
            : HashPassword(request.Password);

        var updated = new UserModel
        {
            Id = id,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        await _userService.UpdateUserAsync(id, updated);

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
    public async Task<ActionResult> Delete(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != id) return Forbid();

        await _userService.DeleteUserAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Computes a SHA-256 hash for the provided password.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>Base64-encoded SHA-256 hash.</returns>
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
