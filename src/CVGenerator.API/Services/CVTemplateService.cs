using CVGeneratorAPI.Models;
using CVGeneratorAPI.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CVGeneratorAPI.Services;

public class CVTemplateService
{
    private readonly IMongoCollection<CVTemplateModel> _col;

    public CVTemplateService(IOptions<MongoDBSettings> mongoSettings)
    {
        var client = new MongoClient(mongoSettings.Value.ConnectionString);
        var db = client.GetDatabase(mongoSettings.Value.DatabaseName);
        _col = db.GetCollection<CVTemplateModel>(mongoSettings.Value.TemplatesCollectionName ?? "cv_templates");

        // Helpful indexes
        var idx = new[]
        {
            new CreateIndexModel<CVTemplateModel>(Builders<CVTemplateModel>.IndexKeys.Ascending(x => x.Name)),
            new CreateIndexModel<CVTemplateModel>(Builders<CVTemplateModel>.IndexKeys.Ascending(x => x.Engine)),
            new CreateIndexModel<CVTemplateModel>(Builders<CVTemplateModel>.IndexKeys.Ascending(x => x.Tags))
        };
        _col.Indexes.CreateMany(idx);
    }

    public async Task<List<CVTemplateModel>> GetAllAsync(
        TemplateEngine? engine = null,
        bool? activeOnly = null,
        string? search = null,
        string[]? tags = null)
    {
        var filter = Builders<CVTemplateModel>.Filter.Empty;

        if (engine is not null)
            filter &= Builders<CVTemplateModel>.Filter.Eq(x => x.Engine, engine);

        if (activeOnly == true)
            filter &= Builders<CVTemplateModel>.Filter.Eq(x => x.IsActive, true);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var rx = new BsonRegularExpression(search, "i");
            filter &= Builders<CVTemplateModel>.Filter.Or(
                Builders<CVTemplateModel>.Filter.Regex(x => x.Name, rx),
                Builders<CVTemplateModel>.Filter.Regex(x => x.Description, rx)
            );
        }

        if (tags is { Length: > 0 })
            filter &= Builders<CVTemplateModel>.Filter.All(x => x.Tags, tags);

        return await _col.Find(filter).SortBy(x => x.Name).ToListAsync();
    }

    public async Task<CVTemplateModel?> GetByIdAsync(string id) =>
        await _col.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<CVTemplateModel> CreateAsync(CVTemplateModel model)
    {
        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;
        await _col.InsertOneAsync(model);
        return model;
    }

    public async Task<bool> UpdateAsync(string id, Action<CVTemplateModel> update)
    {
        var doc = await GetByIdAsync(id);
        if (doc is null) return false;
        update(doc);
        await _col.ReplaceOneAsync(x => x.Id == id, doc);
        return true;
    }

    public Task<long> DeleteAsync(string id) =>
        _col.DeleteOneAsync(x => x.Id == id).ContinueWith(t => t.Result.DeletedCount);
}
