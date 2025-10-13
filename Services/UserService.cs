using System.Linq.Expressions;
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

        // (Optional but recommended) Ensure indexes for quick lookups & uniqueness
        // CreateIndexesIfMissing(); // uncomment if you want to run at startup
    }

    public async Task<UserModel?> GetByUsernameAsync(string username) =>
        await _userCollection.Find(u => u.Username == username).FirstOrDefaultAsync();

    public async Task<UserModel?> GetByEmailAsync(string email) =>
        await _userCollection.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<UserModel?> GetByIdAsync(string id) =>
        await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task CreateUserAsync(UserModel user)
    {
        var now = DateTime.UtcNow;
        user.CreatedAt = now;
        user.UpdatedAt = now;
        await _userCollection.InsertOneAsync(user);
    }

    //  Atomic, partial profile update
   public async Task<UserModel> UpdateProfileAsync(string id, Action<ProfileUpdateBuilder> configure)
    {
        var builder = new ProfileUpdateBuilder();
        configure(builder);

        // always touch UpdatedAt
        builder.Touch();

        var update = builder.ToUpdate(); // Combine into a single UpdateDefinition

        var options = new FindOneAndUpdateOptions<UserModel>
        {
            ReturnDocument = ReturnDocument.After
        };

        var updated = await _userCollection.FindOneAndUpdateAsync(u => u.Id == id, update, options);
        if (updated is null) throw new KeyNotFoundException("User not found");
        return updated;
    }

    // Change password (verifies current hash externally or in controller)
    public async Task UpdatePasswordAsync(string id, string newPlaintextPassword, bool bumpTokenVersion = true)
    {
        var newHash = _hasher.Hash(newPlaintextPassword);

        var update = Builders<UserModel>.Update
            .Set(u => u.PasswordHash, newHash)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        if (bumpTokenVersion)
            update = update.Inc(u => u.TokenVersion, 1);

        var result = await _userCollection.UpdateOneAsync(u => u.Id == id, update);
        if (result.MatchedCount == 0) throw new KeyNotFoundException("User not found");
    }

    // Set avatar URL
    public async Task<UserModel> SetAvatarAsync(string id, string avatarUrl)
    {
        var update = Builders<UserModel>.Update
            .Set(u => u.AvatarUrl, avatarUrl)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<UserModel> { ReturnDocument = ReturnDocument.After };
        var updated = await _userCollection.FindOneAndUpdateAsync(u => u.Id == id, update, options);
        if (updated is null) throw new KeyNotFoundException("User not found");
        return updated;
    }

    // Forgot-password: create token + expiry
    public async Task SetPasswordResetAsync(string id, string token, DateTime expiresUtc)
    {
        var update = Builders<UserModel>.Update
            .Set(u => u.PasswordResetToken, token)
            .Set(u => u.PasswordResetExpiresAt, expiresUtc)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await _userCollection.UpdateOneAsync(u => u.Id == id, update);
        if (result.MatchedCount == 0) throw new KeyNotFoundException("User not found");
    }

    public async Task<UserModel?> GetByPasswordResetTokenAsync(string token) =>
        await _userCollection.Find(u => u.PasswordResetToken == token).FirstOrDefaultAsync();

    // Consume token, set new password, invalidate other sessions
    public async Task ResetPasswordWithTokenAsync(string id, string newPlaintextPassword)
    {
        var newHash = _hasher.Hash(newPlaintextPassword);

        var update = Builders<UserModel>.Update
            .Set(u => u.PasswordHash, newHash)
            .Unset(u => u.PasswordResetToken)
            .Unset(u => u.PasswordResetExpiresAt)
            .Inc(u => u.TokenVersion, 1)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await _userCollection.UpdateOneAsync(u => u.Id == id, update);
        if (result.MatchedCount == 0) throw new KeyNotFoundException("User not found");
    }

    public Task DeleteUserAsync(string id) =>
        _userCollection.DeleteOneAsync(u => u.Id == id);

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

    // -----------------------
    // Helpers / builders
    // -----------------------

    public sealed class ProfileUpdateBuilder
    {
        private readonly List<UpdateDefinition<UserModel>> _defs = new();

        public ProfileUpdateBuilder SetIfNotNull<TField>(TField? value, Expression<Func<UserModel, TField>> field)
        {
            if (value is not null)
                _defs.Add(Builders<UserModel>.Update.Set(field, value));
            return this;
        }

        // For collections: replace only if a value is provided (null = leave as-is)
        public ProfileUpdateBuilder ReplaceListIfProvided<TItem>(
            IEnumerable<TItem>? value,
            Expression<Func<UserModel, IEnumerable<TItem>>> field)
        {
            if (value is not null)
                _defs.Add(Builders<UserModel>.Update.Set(field, value));
            return this;
        }

        // Convenience to always bump UpdatedAt
        public ProfileUpdateBuilder Touch()
        {
            _defs.Add(Builders<UserModel>.Update.Set(u => u.UpdatedAt, DateTime.UtcNow));
            return this;
        }

        // Finalize into a single UpdateDefinition<UserModel>
        public UpdateDefinition<UserModel> ToUpdate()
        {
            if (_defs.Count == 0)
                throw new InvalidOperationException("No updates specified.");

            return _defs.Count == 1
                ? _defs[0]
                : Builders<UserModel>.Update.Combine(_defs);
        }
    }

    // (Optional) create indexes (run once at startup)
    private void CreateIndexesIfMissing()
    {
        var keys1 = Builders<UserModel>.IndexKeys.Ascending(u => u.Email);
        var keys2 = Builders<UserModel>.IndexKeys.Ascending(u => u.Username);
        var optionsUnique = new CreateIndexOptions { Unique = true, Background = true, Sparse = true };
        _userCollection.Indexes.CreateMany(new[]
        {
            new CreateIndexModel<UserModel>(keys1, optionsUnique),
            new CreateIndexModel<UserModel>(keys2, optionsUnique)
        });
    }
}
