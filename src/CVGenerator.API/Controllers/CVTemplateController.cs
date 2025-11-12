using System.Linq;
using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Mappers;
using CVGeneratorAPI.Models;
using CVGeneratorAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVGeneratorAPI.Controllers;

/// <summary>
/// REST endpoints for managing CV templates (both React-schema and Markup engines).
/// </summary>
/// <remarks>
/// Base route: <c>/api/cv-templates</c>
///
/// <para>
/// Supports:
/// <list type="bullet">
///   <item><description>Listing templates with optional filters (engine, active, search, tags).</description></item>
///   <item><description>Fetching a single template with full definition (markup/css or tokens/layout).</description></item>
///   <item><description>Creating/updating/deleting templates (Admin only).</description></item>
/// </list>
/// </para>
///
/// <para>
/// Example: list templates filtered by engine and active state:
/// <code>
/// GET /api/cv-templates?engine=ReactSchema&amp;activeOnly=true
/// </code>
/// </para>
/// </remarks>
[ApiController]
[Authorize]
[Produces("application/json")]
[Route("api/cv-templates")]
[Tags("CV Templates")]
public class CVTemplateController : ControllerBase
{
    private readonly CVTemplateService _service;
    private readonly ILogger<CVTemplateController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CVTemplateController"/>.
    /// </summary>
    /// <param name="service">Template persistence service.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public CVTemplateController(CVTemplateService service, ILogger<CVTemplateController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// List templates with optional filters.
    /// </summary>
    /// <remarks>
    /// Returns a lightweight set of fields suitable for template galleries/lists.
    ///
    /// <para>Query parameters:</para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Parameter</term><description>Description</description>
    ///   </listheader>
    ///   <item><term>engine</term><description>Filter by rendering engine (<c>ReactSchema</c> or <c>Markup</c>).</description></item>
    ///   <item><term>activeOnly</term><description>When <c>true</c>, returns only active templates.</description></item>
    ///   <item><term>search</term><description>Case-insensitive substring match on <c>Name</c> and <c>Description</c>.</description></item>
    ///   <item><term>tags</term><description>Require templates to include all specified tags.</description></item>
    /// </list>
    ///
    /// <para>Example:</para>
    /// <code>
    /// GET /api/cv-templates?engine=Markup&amp;activeOnly=true&amp;search=modern&amp;tags=two-column&amp;tags=ats
    /// </code>
    /// </remarks>
    /// <param name="engine">Optional engine to filter by.</param>
    /// <param name="activeOnly">If true, only active templates are returned.</param>
    /// <param name="search">Optional search term for name/description.</param>
    /// <param name="tags">Optional list of tags; templates must contain all of them.</param>
    /// <returns>List of templates (list item shape).</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CVTemplateListItemResponse>), 200)]
    public async Task<ActionResult<List<CVTemplateListItemResponse>>> GetAll(
        [FromQuery] TemplateEngine? engine,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search,
        [FromQuery] string[]? tags)
    {
        var items = await _service.GetAllAsync(engine, activeOnly, search, tags);
        return Ok(items.Select(x => x.ToListItem()).ToList());
    }

    /// <summary>
    /// Get a single template by id (detailed view).
    /// </summary>
    /// <remarks>
    /// Returns the full definition of the template, including engine-specific fields:
    /// <list type="bullet">
    ///   <item><description><c>Markup</c>: <c>Markup</c> (HTML) and optional <c>Css</c>.</description></item>
    ///   <item><description><c>ReactSchema</c>: <c>Tokens</c> (style map) and <c>Layout</c> (layout DSL).</description></item>
    /// </list>
    ///
    /// <para>Example:</para>
    /// <code>
    /// GET /api/cv-templates/64f1c6c5f0a1a9b0e3c12345
    /// </code>
    /// </remarks>
    /// <param name="id">Template MongoDB identifier.</param>
    /// <returns>The detailed template definition, or <c>404</c> if not found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CVTemplateDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CVTemplateDetailResponse>> GetById(string id)
    {
        var doc = await _service.GetByIdAsync(id);
        if (doc is null) return NotFound();
        return Ok(doc.ToDetail());
    }

    /// <summary>
    /// Create a new template (Admin only).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provide engine-specific fields:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><c>Markup</c> rarr; <c>Markup</c> (required) and optional <c>Css</c>.</description></item>
    ///   <item><description><c>ReactSchema</c> rarr; <c>Layout</c> (required) and optional <c>Tokens</c>.</description></item>
    /// </list>
    ///
    /// <para>On success, the <c>Location</c> header points to <c>GET /api/cv-templates/{id}</c>.</para>
    /// </remarks>
    /// <param name="req">Template payload.</param>
    /// <returns>The created template definition.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CVTemplateDetailResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<CVTemplateDetailResponse>> Create([FromBody] CreateCVTemplateRequest req)
    {
        // Basic engine-specific validation
        if (req.Engine == TemplateEngine.Markup && string.IsNullOrWhiteSpace(req.Markup))
            return ValidationProblem("Markup templates require 'Markup' HTML.");

        if (req.Engine == TemplateEngine.ReactSchema && req.Layout is null)
            return ValidationProblem("React-schema templates require 'Layout' JSON.");

        var model = req.FromCreate();
        var created = await _service.CreateAsync(model);

        _logger.LogInformation("Template created: {TemplateId} ({Name}, Engine={Engine})",
            created.Id, created.Name, created.Engine);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDetail());
    }

    /// <summary>
    /// Update an existing template (Admin only).
    /// </summary>
    /// <remarks>
    /// Partial updates are supported—only provided fields are applied.  
    /// If switching <see cref="TemplateEngine"/>, also provide the target engine's required fields.
    ///
    /// <para>Example:</para>
    /// <code>
    /// PUT /api/cv-templates/64f1c6c5f0a1a9b0e3c12345
    /// { "description": "Updated", "tags": ["two-column","modern"] }
    /// </code>
    /// </remarks>
    /// <param name="id">Template MongoDB identifier.</param>
    /// <param name="req">Fields to update.</param>
    /// <returns><c>204 No Content</c> on success; <c>404</c> if not found.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("application/json")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCVTemplateRequest req)
    {
        var ok = await _service.UpdateAsync(id, m => m.ApplyUpdate(req));
        if (!ok) return NotFound();

        _logger.LogInformation("Template updated: {TemplateId}", id);
        return NoContent();
    }

    /// <summary>
    /// Delete a template (Admin only).
    /// </summary>
    /// <remarks>
    /// Consider toggling <c>IsActive</c> instead of hard deletion when you want to
    /// prevent usage but keep history and version references.
    /// </remarks>
    /// <param name="id">Template MongoDB identifier.</param>
    /// <returns><c>204 No Content</c> on success; <c>404</c> if not found.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (deleted == 0) return NotFound();

        _logger.LogWarning("Template deleted: {TemplateId}", id);
        return NoContent();
    }
}
