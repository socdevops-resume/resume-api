using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MongoDB.Driver;
using Xunit;

[CollectionDefinition("mongo", DisableParallelization = true)]
public class MongoCollectionDefinition : ICollectionFixture<MongoFixture> { }

public sealed class MongoFixture : IAsyncLifetime
{
    private TestcontainersContainer? _mongo;
    public IMongoDatabase Db { get; private set; } = default!;
    public IMongoDatabase Database => Db; // back-compat alias
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Use external DB if provided (handy for local dev)
        var external = Environment.GetEnvironmentVariable("MONGODB_URI");
        if (!string.IsNullOrWhiteSpace(external))
        {
            var url = new MongoUrl(external);
            ConnectionString = external;
            Db = new MongoClient(ConnectionString).GetDatabase(url.DatabaseName ?? "cv-tests");
            return;
        }

        // 1.x API: use TestcontainersBuilder<TestcontainersContainer>
        _mongo = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo:7")
            .WithPortBinding(0, 27017) // random host port -> 27017
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
            .Build();

        await _mongo.StartAsync();

        var host = "localhost";
        var port = _mongo.GetMappedPublicPort(27017);
        ConnectionString = $"mongodb://{host}:{port}";
        Db = new MongoClient(ConnectionString).GetDatabase("cv-tests");
    }

    public async Task DisposeAsync()
    {
        if (_mongo is not null)
        {
            await _mongo.DisposeAsync();
        }
    }
}
