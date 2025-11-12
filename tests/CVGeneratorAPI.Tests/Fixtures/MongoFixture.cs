// tests/CVGeneratorAPI.Tests/Fixtures/MongoFixture.cs
using Mongo2Go;
using MongoDB.Driver;
using Xunit;

public class MongoFixture : IDisposable
{
    public MongoDbRunner Runner { get; }
    public IMongoDatabase Database { get; }

    public MongoFixture()
    {
        Runner = MongoDbRunner.Start(singleNodeReplSet: true); // enables findOneAndUpdate
        var client = new MongoClient(Runner.ConnectionString);
        Database = client.GetDatabase("cvgen_testdb");
    }

    public void Dispose() => Runner.Dispose();
}

[CollectionDefinition("mongo")]
public class MongoCollection : ICollectionFixture<MongoFixture> { }
