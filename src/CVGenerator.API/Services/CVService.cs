using System.Linq.Expressions;
using CVGeneratorAPI.Models;
using CVGeneratorAPI.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CVGeneratorAPI.Services;

public class CVService
{
    private readonly IMongoCollection<CVModel> _cvCollection;

    public CVService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _cvCollection = database.GetCollection<CVModel>(mongoDBSettings.Value.CVsCollectionName);

        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        // Single-field for listing CVs for a user
        var byUser = new CreateIndexModel<CVModel>(
            Builders<CVModel>.IndexKeys.Ascending(x => x.UserId),
            new CreateIndexOptions { Background = true });

        // Compound for quick retrieval of a specific CV owned by user
        var byUserAndId = new CreateIndexModel<CVModel>(
            Builders<CVModel>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Id),
            new CreateIndexOptions { Background = true });

        _cvCollection.Indexes.CreateMany(new[] { byUser, byUserAndId });
    }

    // CREATE
    public Task CreateCvAsync(CVModel cv)
    {
        var now = DateTime.UtcNow;
        cv.CreatedAt = now;
        cv.UpdatedAt = now;
        return _cvCollection.InsertOneAsync(cv);
    }

    // READ
    public Task<List<CVModel>> GetAllByUserAsync(string userId) =>
        _cvCollection.Find(c => c.UserId == userId).ToListAsync();

    public Task<CVModel?> GetByIdForUserAsync(string id, string userId) =>
        _cvCollection.Find(c => c.Id == id && c.UserId == userId).FirstOrDefaultAsync() as Task<CVModel?>;

    // UPDATE (partial, atomic)
    public async Task<CVModel?> UpdatePartialForUserAsync(string id, string userId, Action<CvUpdateBuilder> configure)
    {
        var b = new CvUpdateBuilder();
        configure(b);
        b.Touch(); // sets UpdatedAt
        var update = b.ToUpdate();

        var options = new FindOneAndUpdateOptions<CVModel> { ReturnDocument = ReturnDocument.After };
        return await _cvCollection.FindOneAndUpdateAsync(c => c.Id == id && c.UserId == userId, update, options);
    }

    // DELETE
    public Task DeleteForUserAsync(string id, string userId) =>
        _cvCollection.DeleteOneAsync(c => c.Id == id && c.UserId == userId);

    // --------- builder ---------
    public sealed class CvUpdateBuilder
    {
        private readonly List<UpdateDefinition<CVModel>> _defs = new();

        public CvUpdateBuilder SetIfNotNull<T>(T? value, Expression<Func<CVModel, T>> field)
        {
            if (value is not null) _defs.Add(Builders<CVModel>.Update.Set(field, value));
            return this;
        }

        public CvUpdateBuilder ReplaceListIfProvided<T>(IEnumerable<T>? value, Expression<Func<CVModel, IEnumerable<T>>> field)
        {
            if (value is not null) _defs.Add(Builders<CVModel>.Update.Set(field, value));
            return this;
        }

        public CvUpdateBuilder Touch()
        {
            _defs.Add(Builders<CVModel>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow));
            return this;
        }

        public UpdateDefinition<CVModel> ToUpdate()
        {
            if (_defs.Count == 0) throw new InvalidOperationException("No updates specified.");
            return _defs.Count == 1 ? _defs[0] : Builders<CVModel>.Update.Combine(_defs);
        }
    }
}
