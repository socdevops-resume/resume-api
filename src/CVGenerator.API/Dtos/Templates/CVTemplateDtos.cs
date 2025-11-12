using System.Text.Json;
using CVGeneratorAPI.Models;

namespace CVGeneratorAPI.Dtos;

/// <summary>
/// Lightweight DTO for listing CV templates (e.g., in a gallery).
/// </summary>
/// <remarks>
/// Intended for scenarios where engine-specific payload (markup, tokens, layout)
/// is not required. Use <see cref="CVTemplateDetailResponse"/> when the full
/// template definition is needed.
/// </remarks>
public class CVTemplateListItemResponse
{
    /// <summary>
    /// MongoDB document identifier of the template.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Human-readable template name shown in the UI.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Short description used in lists and tooltips.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Rendering engine used by this template (React schema or Markup).
    /// </summary>
    public required TemplateEngine Engine { get; set; }

    /// <summary>
    /// Semantic version of the template definition (e.g., <c>1.0.0</c>).
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Optional tags to filter/search templates (e.g., <c>["modern","two-column"]</c>).
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Absolute or relative URL of a pre-generated preview image for fast gallery display.
    /// </summary>
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// Indicates whether the template can be selected by end users.
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Full template definition returned for admin/edit screens and for rendering.
/// </summary>
/// <remarks>
/// This DTO extends <see cref="CVTemplateListItemResponse"/> with engine-specific
/// fields and metadata used at render time.
/// </remarks>
public class CVTemplateDetailResponse : CVTemplateListItemResponse
{
    /// <summary>
    /// HTML markup with placeholders when Engine is <c>Markup</c>.
    /// </summary>
    /// <remarks>
    /// Recommended placeholder syntax is Handlebars/Mustache (e.g., <c>{{firstName}}</c>,
    /// <c>{{#each skills}}…{{/each}}</c>). This string is compiled and bound to user CV
    /// data before export/preview.
    /// </remarks>
    public string? Markup { get; set; }

    /// <summary>
    /// Optional CSS applied together with <see cref="Markup"/> for <c>Markup</c> templates.
    /// </summary>
    public string? Css { get; set; }

    /// <summary>
    /// Style token map used by <c>ReactSchema</c> templates (JSON object).
    /// </summary>
    /// <remarks>
    /// Typical content is a key→className dictionary (e.g., Tailwind classes).
    /// <example>
    /// { "title": "text-3xl font-semibold", "sectionTitle": "text-lg font-bold", "body": "text-sm" }
    /// </example>
    /// </remarks>
    public JsonDocument? Tokens { get; set; }

    /// <summary>
    /// Layout description for <c>ReactSchema</c> templates (JSON array).
    /// </summary>
    /// <remarks>
    /// The layout is a small DSL consumed by the frontend renderer.
    /// <example>
    /// [ { "type":"header","fields":["firstName","lastName","email","phone"] },
    ///   { "type":"twoColumn","left":[{"type":"section","title":"Skills","bind":"skills[]","renderer":"bullets"}],
    ///     "right":[{"type":"section","title":"Experience","bind":"experiences[]"}] } ]
    /// </example>
    /// </remarks>
    public JsonDocument? Layout { get; set; }

    /// <summary>
    /// The list of data variables the template expects (e.g., <c>["firstName","skills[]"]</c>).
    /// </summary>
    public string[] Variables { get; set; } = Array.Empty<string>();

    /// <summary>
    /// UTC timestamp when the template was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp of the last modification.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request payload for creating a new CV template.
/// </summary>
/// <remarks>
/// Provide engine-specific fields based on <see cref="Engine"/>:
/// <list type="bullet">
/// <item><description><c>Markup</c>: set <see cref="Markup"/> (required) and optional <see cref="Css"/>.</description></item>
/// <item><description><c>ReactSchema</c>: set <see cref="Layout"/> (required) and optional <see cref="Tokens"/>.</description></item>
/// </list>
/// </remarks>
public class CreateCVTemplateRequest
{
    /// <summary>
    /// Human-readable template name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Short description to help users choose a template.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Rendering engine for this template.
    /// </summary>
    public required TemplateEngine Engine { get; set; }

    /// <summary>
    /// HTML markup with placeholders (required when <see cref="Engine"/> is <c>Markup</c>).
    /// </summary>
    public string? Markup { get; set; }

    /// <summary>
    /// Optional CSS used together with <see cref="Markup"/>.
    /// </summary>
    public string? Css { get; set; }

    /// <summary>
    /// Token dictionary for styling (JSON object). Used when <see cref="Engine"/> is <c>ReactSchema</c>.
    /// </summary>
    /// <remarks>
    /// Example: { "title": "text-3xl", "body": "text-sm leading-6" }
    /// </remarks>
    public JsonDocument? Tokens { get; set; }

    /// <summary>
    /// Layout JSON for <c>ReactSchema</c> (required when <see cref="Engine"/> is <c>ReactSchema</c>).
    /// </summary>
    /// <remarks>
    /// Must be a JSON array describing the component tree to render.
    /// </remarks>
    public JsonDocument? Layout { get; set; }

    /// <summary>
    /// Template semantic version (defaults to <c>1.0.0</c>).
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Variables this template expects to receive from CV data.
    /// </summary>
    public string[] Variables { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Optional tags used for filtering/grouping in the UI.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Optional URL of a preview image used in template galleries.
    /// </summary>
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// Whether the template is active immediately after creation (default: <c>true</c>).
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request payload for updating an existing CV template.
/// </summary>
/// <remarks>
/// All properties are optional; only provided fields will be updated.  
/// When switching <see cref="Engine"/>, provide the corresponding engine-specific
/// fields (<see cref="Markup"/>/<see cref="Css"/> or <see cref="Layout"/>/<see cref="Tokens"/>).
/// </remarks>
public class UpdateCVTemplateRequest
{
    /// <summary>
    /// New template name (optional).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// New description (optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Change the rendering engine (optional). Ensure matching engine fields are supplied.
    /// </summary>
    public TemplateEngine? Engine { get; set; }

    /// <summary>
    /// Updated HTML markup (for <c>Markup</c> engine).
    /// </summary>
    public string? Markup { get; set; }

    /// <summary>
    /// Updated CSS (for <c>Markup</c> engine).
    /// </summary>
    public string? Css { get; set; }

    /// <summary>
    /// Updated token dictionary (for <c>ReactSchema</c> engine).
    /// </summary>
    public JsonDocument? Tokens { get; set; }

    /// <summary>
    /// Updated layout JSON (for <c>ReactSchema</c> engine).
    /// </summary>
    public JsonDocument? Layout { get; set; }

    /// <summary>
    /// New template semantic version (optional).
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Replace the expected variables list (optional).
    /// </summary>
    public string[]? Variables { get; set; }

    /// <summary>
    /// Replace the tags list (optional).
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// New preview image URL (optional).
    /// </summary>
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// Toggle active state (optional).
    /// </summary>
    public bool? IsActive { get; set; }
}
