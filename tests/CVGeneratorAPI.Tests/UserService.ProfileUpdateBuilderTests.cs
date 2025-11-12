using CVGeneratorAPI.Services;
using CVGeneratorAPI.Models;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;

public class ProfileUpdateBuilderTests
{
    [Fact]
    public void ToUpdate_Throws_When_NoUpdatesSpecified()
    {
        var b = new UserService.ProfileUpdateBuilder();
        var act = () => b.ToUpdate();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetIfNotNull_AddsUpdate_Only_When_Value_NotNull()
    {
        var b = new UserService.ProfileUpdateBuilder();
        b.SetIfNotNull("John", u => u.Username).Touch();
        var update = b.ToUpdate();
        update.Should().NotBeNull();
    }

    [Fact]
    public void ReplaceListIfProvided_AddsUpdate_Only_When_Provided()
    {
        var b = new UserService.ProfileUpdateBuilder();
        b.ReplaceListIfProvided<string>(new[] { "Admin" }, u => u.Roles)
         .Touch();
        var update = b.ToUpdate();
        update.Should().NotBeNull();
    }
}
