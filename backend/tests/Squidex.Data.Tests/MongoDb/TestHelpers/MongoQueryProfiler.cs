// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.MongoDb.TestHelpers;

public sealed record QueryProfile
{
    public string Operation { get; init; }

    public string PlanSummary { get; init; }

    public bool HasSortStage { get; init; }

    public long KeysExamined { get; init; }

    public long DocsExamined { get; init; }

    public long Returned { get; init; }

    public static QueryProfile Parse(BsonDocument document)
    {
        long GetNumber(string name)
        {
            return document.TryGetValue(name, out var value) ? value.ToInt64() : 0;
        }

        return new QueryProfile
        {
            Operation = document["op"].AsString,
            PlanSummary = document["planSummary"].AsString,
            HasSortStage = document.TryGetValue("hasSortStage", out var sort) && sort.ToBoolean(),
            KeysExamined = GetNumber("keysExamined"),
            DocsExamined = GetNumber("docsExamined"),
            Returned = GetNumber("nreturned"),
        };
    }
}

public sealed class MongoQueryProfiler(IMongoDatabase database, string collectionName)
{
    private const string ProfileCollection = "system.profile";
    private const string PlanByDocumentId = "IXSCAN { _id: 1 }";
    private readonly List<QueryProfile> recorded = [];

    public IReadOnlyList<QueryProfile> Recorded => recorded;

    public async Task StartAsync()
    {
        await database.RunCommandAsync<BsonDocument>("{ profile: 0 }");
        await database.DropCollectionAsync(ProfileCollection);

        // The profile collection is capped, so make it large enough for all queries of a single test.
        await database.CreateCollectionAsync(ProfileCollection, new CreateCollectionOptions { Capped = true, MaxSize = 64 * 1024 * 1024 });

        // The profiler is enabled per database and only records the reads of the profiled collection. Level 2 would ignore the filter.
        var command = new BsonDocument
        {
            ["profile"] = 1,
            ["filter"] = new BsonDocument
            {
                ["ns"] = $"{database.DatabaseNamespace.DatabaseName}.{collectionName}",
                ["op"] = new BsonDocument("$in", new BsonArray { "query", "getmore", "command" }),
            },
        };

        await database.RunCommandAsync<BsonDocument>(command);
    }

    public async Task StopAsync()
    {
        await CollectAsync();
        await database.RunCommandAsync<BsonDocument>("{ profile: 0 }");
    }

    public ValueTask AssertIndexedAsync()
    {
        foreach (var profile in recorded)
        {
            // Queries by IDs are sorted in memory, but the number of results is bounded by the IDs.
            AssertIndexed(profile, allowSort: profile.PlanSummary == PlanByDocumentId);
        }

        return default;
    }

    public static void AssertIndexed(QueryProfile profile, bool allowSort = false)
    {
        Assert.False(profile.PlanSummary.Contains("COLLSCAN", StringComparison.Ordinal), $"Collection scan: {profile}");

        if (!allowSort)
        {
            Assert.False(profile.HasSortStage, $"Blocking sort: {profile}");
        }
    }

    public async Task<List<QueryProfile>> ProfileAsync(Func<Task> action)
    {
        await CollectAsync();
        await action();

        return await CollectAsync();
    }

    private async Task<List<QueryProfile>> CollectAsync()
    {
        var documents =
            await database.GetCollection<BsonDocument>(ProfileCollection)
                .Find(new BsonDocument())
                .ToListAsync();

        var profiles = documents.Where(x => x.Contains("planSummary")).Select(QueryProfile.Parse).ToList();

        recorded.AddRange(profiles);

        // Start from scratch, so that the next collection only returns new queries.
        await StartAsync();
        return profiles;
    }
}
