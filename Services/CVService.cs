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
    }

    // ---------- CREATE ----------
    public Task CreateCvAsync(CVModel cv) =>
        _cvCollection.InsertOneAsync(cv);

    // ---------- READ ----------
    public Task<List<CVModel>> GetAllByUserAsync(string userId) =>
        _cvCollection.Find(c => c.UserId == userId).ToListAsync();


    public async Task<CVModel?> GetByIdForUserAsync(string id, string userId)
    {
        var result = await _cvCollection
            .Find(c => c.Id == id && c.UserId == userId)
            .FirstOrDefaultAsync();

        // 'result' may be null at runtime; returning CVModel? is correct.
        return result;
    }

    // ---------- UPDATE ----------
    public Task UpdateForUserAsync(string id, string userId, CVModel cv) =>
        _cvCollection.ReplaceOneAsync(c => c.Id == id && c.UserId == userId, cv);

    // ---------- DELETE ----------
    public Task DeleteForUserAsync(string id, string userId) =>
        _cvCollection.DeleteOneAsync(c => c.Id == id && c.UserId == userId);
}
