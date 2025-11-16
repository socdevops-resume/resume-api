// tests/CVGeneratorAPI.Tests/UserService.IntegrationTests.cs
using System.Threading.Tasks;
using CVGeneratorAPI.Models;
using CVGeneratorAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Driver;
using Xunit;

[Collection("mongo")]
public class UserServiceIntegrationTests
{
    private readonly IMongoCollection<UserModel> _users;
    private readonly Mock<IPasswordHasher> _hasherMock;

    public UserServiceIntegrationTests(MongoFixture fx)
    {
        _users = fx.Database.GetCollection<UserModel>("users");
        _users.DeleteMany(_ => true); // clean slate per test class

        _hasherMock = new Mock<IPasswordHasher>();
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns((string s) => $"HASH::{s}");
    }

    private UserService CreateService() => new UserService(_users, _hasherMock.Object);

    [Fact]
    public async Task CreateUser_SetsTimestamps_AndInserts()
    {
        var svc = CreateService();
        var u = new UserModel { Username = "john", Email = "j@e.com", PasswordHash = "x" };

        await svc.CreateUserAsync(u);

        var found = await _users.Find(x => x.Username == "john").FirstOrDefaultAsync();
        found.Should().NotBeNull();
        found!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        found.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdatePassword_BumpsTokenVersion_WhenRequested()
    {
        var svc = CreateService();
        var u = new UserModel { Username = "u1", Email = "u1@e", PasswordHash = "old", TokenVersion = 3 };
        await _users.InsertOneAsync(u);

        await svc.UpdatePasswordAsync(u.Id!, "NewPass!", bumpTokenVersion: true);

        var updated = await _users.Find(x => x.Id == u.Id).FirstAsync();
        updated.PasswordHash.Should().Be("HASH::NewPass!");
        updated.TokenVersion.Should().Be(4);
        updated.UpdatedAt.Should().BeAfter(u.UpdatedAt);
    }

    [Fact]
    public async Task SetAvatar_ReturnsUpdatedUser()
    {
        var svc = CreateService();
        var u = new UserModel { Username = "pic", Email = "p@e", PasswordHash = "z" };
        await _users.InsertOneAsync(u);

        var after = await svc.SetAvatarAsync(u.Id!, "https://cdn/avatar.png");

        after.AvatarUrl.Should().Be("https://cdn/avatar.png");
        after.UpdatedAt.Should().BeAfter(u.UpdatedAt);
    }

    [Fact]
    public async Task ResetPasswordWithToken_ClearsToken_And_IncrementsVersion()
    {
        var svc = CreateService();
        var u = new UserModel {
            Username = "reset",
            Email = "r@e",
            PasswordHash = "old",
            TokenVersion = 0,
            PasswordResetToken = "t1",
            PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        await _users.InsertOneAsync(u);

        await svc.ResetPasswordWithTokenAsync(u.Id!, "N3w!");

        var updated = await _users.Find(x => x.Id == u.Id).FirstAsync();
        updated.PasswordHash.Should().Be("HASH::N3w!");
        updated.PasswordResetToken.Should().BeNull();
        updated.PasswordResetExpiresAt.Should().BeNull();
        updated.TokenVersion.Should().Be(1);
    }

    // [Fact]
    // public async Task EnsureAdminUser_Creates_WhenMissing()
    // {
    //     var svc = CreateService();
    //     await svc.EnsureAdminUserAsync();

    //     var admin = await _users.Find(x => x.Email == "ovidiu.suciusoc@gmail.com").FirstOrDefaultAsync();
    //     admin.Should().NotBeNull();
    //     admin!.Roles.Should().Contain("Admin");
    //     _hasherMock.Verify(h => h.Hash("Admin123!"), Times.Once);
    // }
}
