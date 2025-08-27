using System.Text.Json;
using CVGeneratorAPI.Dtos;
using CVGeneratorAPI.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace CVGeneratorAPI.Mappers;

public static class CVTemplateMappers
{
    // Helpers to convert JsonDocument <-> Bson
    private static BsonDocument? ToBsonDocument(JsonDocument? json) =>
    json is null ? null : BsonSerializer.Deserialize<BsonDocument>(json.RootElement.GetRawText());
    
    private static BsonArray? ToBsonArray(JsonDocument? json) =>
        json is null ? null : BsonSerializer.Deserialize<BsonArray>(json.RootElement.GetRawText());

    private static JsonDocument? ToJson(BsonValue? bson) =>
        bson is null ? null : JsonDocument.Parse(bson.ToJson());

    public static CVTemplateListItemResponse ToListItem(this CVTemplateModel m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        Engine = m.Engine,
        Version = m.Version,
        Tags = m.Tags,
        PreviewImageUrl = m.PreviewImageUrl,
        IsActive = m.IsActive
    };

    public static CVTemplateDetailResponse ToDetail(this CVTemplateModel m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        Engine = m.Engine,
        Version = m.Version,
        Tags = m.Tags,
        PreviewImageUrl = m.PreviewImageUrl,
        IsActive = m.IsActive,

        // engine-specific
        Markup = m.Markup,
        Css = m.Css,
        Tokens = ToJson(m.Tokens),
        Layout = ToJson(m.Layout),

        Variables = m.Variables,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };

    public static CVTemplateModel FromCreate(this CreateCVTemplateRequest r) => new()
    {
        Name = r.Name,
        Description = r.Description,
        Engine = r.Engine,

        Markup = r.Engine == TemplateEngine.Markup ? r.Markup : null,
        Css = r.Engine == TemplateEngine.Markup ? r.Css : null,

        Tokens = r.Engine == TemplateEngine.ReactSchema ? ToBsonDocument(r.Tokens) : null,
        Layout = r.Engine == TemplateEngine.ReactSchema ? ToBsonArray(r.Layout) : null,

        Version = r.Version,
        Variables = r.Variables,
        Tags = r.Tags,
        PreviewImageUrl = r.PreviewImageUrl,
        IsActive = r.IsActive
    };

    public static void ApplyUpdate(this CVTemplateModel m, UpdateCVTemplateRequest r)
    {
        if (r.Name is not null) m.Name = r.Name;
        if (r.Description is not null) m.Description = r.Description;
        if (r.Engine is not null) m.Engine = r.Engine.Value;

        if (m.Engine == TemplateEngine.Markup)
        {
            if (r.Markup is not null) m.Markup = r.Markup;
            if (r.Css is not null) m.Css = r.Css;
            m.Tokens = null; m.Layout = null;
        }
        else if (m.Engine == TemplateEngine.ReactSchema)
        {
            if (r.Tokens is not null) m.Tokens = ToBsonDocument(r.Tokens);
            if (r.Layout is not null) m.Layout = ToBsonArray(r.Layout);
            m.Markup = null; m.Css = null;
        }

        if (r.Version is not null) m.Version = r.Version;
        if (r.Variables is not null) m.Variables = r.Variables;
        if (r.Tags is not null) m.Tags = r.Tags;
        if (r.PreviewImageUrl is not null) m.PreviewImageUrl = r.PreviewImageUrl;
        if (r.IsActive is not null) m.IsActive = r.IsActive.Value;

        m.UpdatedAt = DateTime.UtcNow;
    }
}
