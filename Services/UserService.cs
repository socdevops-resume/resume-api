using CVGeneratorAPI.Models;
using CVGeneratorAPI.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CVGeneratorAPI.Services;

public class UserService
{
    private readonly IMongoCollection<UserModel> _userCollection;
    private readonly IPasswordHasher _hasher;

    public UserService(IOptions<MongoDBSettings> mongoDBSettings, IPasswordHasher hasher)
    {
        _hasher = hasher;
        var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _userCollection = database.GetCollection<UserModel>(mongoDBSettings.Value.UsersCollectionName);
    }

    public async Task<UserModel?> GetByUsernameAsync(string username) =>
        await _userCollection.Find(u => u.Username == username).FirstOrDefaultAsync();

    public async Task<UserModel?> GetByEmailAsync(string email) =>
        await _userCollection.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task CreateUserAsync(UserModel user)
    {
        var now = DateTime.UtcNow;
        user.CreatedAt = now;
        user.UpdatedAt = now;
        await _userCollection.InsertOneAsync(user);
    }

    public async Task UpdateUserAsync(string id, UserModel user)
    {
        user.Id = id;
        user.UpdatedAt = DateTime.UtcNow;
        await _userCollection.ReplaceOneAsync(u => u.Id == id, user);
    }

    public Task DeleteUserAsync(string id) =>
        _userCollection.DeleteOneAsync(u => u.Id == id);

    public async Task<UserModel?> GetByIdAsync(string id) =>
        await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        
    /// <summary>
    /// Ensure there's at least one admin user in the database.
    /// Call this from Program.cs at startup.
    /// </summary>
    public async Task EnsureAdminUserAsync()
    {
        var adminEmail = "ovidiu.suciusoc@gmail.com";
        var adminUser = await GetByEmailAsync(adminEmail);

        if (adminUser != null) return;
        
        var admin = new UserModel
        {
            Username = "admin",
            Email = adminEmail,
            PasswordHash = _hasher.Hash("Admin123!"), 
            Roles = new[] { "Admin" }          
        };
        await CreateUserAsync(admin);
        
    }

}
