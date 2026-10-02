// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Migrations;
using Squidex.MongoDb.TestHelpers;

namespace Squidex.MongoDb.Migrations;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class DropOldAssetIndexTests(MongoFixture fixture)
{
    private readonly IMongoDatabase database = fixture.Client.GetDatabase($"Test_{Guid.NewGuid()}");

    [Fact]
    public async Task Should_drop_old_index_from_all_shards()
    {
        var shard1 = await CreateCollectionAsync("States_Assets2", withOldIndex: true);
        var shard2 = await CreateCollectionAsync("States_Assets2_1", withOldIndex: true);
        var counts = await CreateCollectionAsync("States_Assets2_Count", withOldIndex: false);

        var sut = new DropOldAssetIndex(database);

        await sut.UpdateAsync(default);

        Assert.Equal(["_id_", "other_1"], await GetIndexNamesAsync(shard1));
        Assert.Equal(["_id_", "other_1"], await GetIndexNamesAsync(shard2));
        Assert.Equal(["_id_", "other_1"], await GetIndexNamesAsync(counts));
    }

    [Fact]
    public async Task Should_not_fail_if_no_asset_collection_exists()
    {
        var sut = new DropOldAssetIndex(database);

        await sut.UpdateAsync(default);
    }

    private async Task<IMongoCollection<BsonDocument>> CreateCollectionAsync(string name, bool withOldIndex)
    {
        var collection = database.GetCollection<BsonDocument>(name);

        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("other")));

        if (withOldIndex)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys
                        .Descending("mt")
                        .Ascending("id")
                        .Ascending("_ai")
                        .Ascending("dl")
                        .Ascending("pi")
                        .Ascending("td")));
        }

        return collection;
    }

    private static async Task<List<string>> GetIndexNamesAsync(IMongoCollection<BsonDocument> collection)
    {
        var indexes = await (await collection.Indexes.ListAsync()).ToListAsync();

        return indexes.Select(x => x["name"].AsString).Order().ToList();
    }
}
