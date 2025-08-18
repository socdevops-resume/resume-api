using System.Security.Claims;
using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Mappers;
using CVGeneratorAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CVGeneratorAPI.Controllers;

/// <summary>
/// Endpoints for creating, reading, updating, and deleting CVs
/// scoped to the authenticated user.
/// </summary>
/// <remarks>
/// All routes are under <c>/api/cvs</c> and require a valid JWT.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/cvs")]
[Tags("CVs")]
public class CVsController : ControllerBase
{
    private readonly CVService _cvService;

    /// <summary>
    /// Initializes a new instance of <see cref="CVsController"/>.
    /// </summary>
    /// <param name="cvService">Service that performs CRUD operations on CVs.</param>
    public CVsController(CVService cvService) => _cvService = cvService;

    /// <summary>
    /// Gets the authenticated user's ID from the JWT.
    /// </summary>
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Retrieves all CVs belonging to the authenticated user.
    /// </summary>
    /// <remarks>
    /// **Route:** <c>GET /api/cvs</c><br/>
    /// **Responses:**
    /// - <c>200 OK</c> with a list of <see cref="CVResponse"/>.
    /// </remarks>
    /// <returns>List of CVs owned by the current user.</returns>
    [HttpGet]
    public async Task<ActionResult<List<CVResponse>>> GetAll()
    {
        var cvs = await _cvService.GetAllByUserAsync(UserId);
        return Ok(cvs.Select(c => c.ToResponse()).ToList());
    }

    /// <summary>
    /// Retrieves a specific CV by its ID for the authenticated user.
    /// </summary>
    /// <remarks>
    /// **Route:** <c>GET /api/cvs/{id}</c><br/>
    /// **Responses:**
    /// - <c>200 OK</c> with the requested <see cref="CVResponse"/>.
    /// - <c>404 Not Found</c> if no CV with that ID exists for the user.
    /// </remarks>
    /// <param name="id">The CV identifier.</param>
    /// <returns>The requested CV if found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<CVResponse>> GetById(string id)
    {
        var cv = await _cvService.GetByIdForUserAsync(id, UserId);
        if (cv == null) return NotFound("CV not found.");
        return Ok(cv.ToResponse());
    }

    /// <summary>
    /// Creates a new CV for the authenticated user.
    /// </summary>
    /// <remarks>
    /// **Route:** <c>POST /api/cvs</c><br/>
    /// **Responses:**
    /// - <c>201 Created</c> with the created <see cref="CVResponse"/>.
    /// </remarks>
    /// <param name="newCv">The CV data used to create the record.</param>
    /// <returns>The created CV, including its generated ID.</returns>
    [HttpPost]
    public async Task<ActionResult<CVResponse>> Create([FromBody] CreateCVRequest newCv)
    {
        var model = newCv.ToModel(UserId);
        await _cvService.CreateCvAsync(model);
        var response = model.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Updates an existing CV that belongs to the authenticated user.
    /// </summary>
    /// <remarks>
    /// **Route:** <c>PUT /api/cvs/{id}</c><br/>
    /// **Responses:**
    /// - <c>200 OK</c> with the updated <see cref="CVResponse"/>.
    /// - <c>404 Not Found</c> if the CV does not exist for the user.
    /// </remarks>
    /// <param name="id">The CV identifier.</param>
    /// <param name="updatedCv">The updated CV values.</param>
    /// <returns>The updated CV if the operation succeeds.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<CVResponse>> Update(string id, [FromBody] UpdateCVRequest updatedCv)
    {
        var existing = await _cvService.GetByIdForUserAsync(id, UserId);
        if (existing == null) return NotFound("CV not found.");

        updatedCv.ApplyToModel(existing);
        existing.Id = id;
        existing.UserId = UserId;

        await _cvService.UpdateForUserAsync(id, UserId, existing);
        return Ok(existing.ToResponse());
    }

    /// <summary>
    /// Deletes a CV that belongs to the authenticated user.
    /// </summary>
    /// <remarks>
    /// **Route:** <c>DELETE /api/cvs/{id}</c><br/>
    /// **Responses:**
    /// - <c>204 No Content</c> if the CV was deleted.
    /// - <c>404 Not Found</c> if the CV does not exist for the user.
    /// </remarks>
    /// <param name="id">The CV identifier.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var existing = await _cvService.GetByIdForUserAsync(id, UserId);
        if (existing == null) return NotFound("CV not found.");

        await _cvService.DeleteForUserAsync(id, UserId);
        return NoContent();
    }
}
