// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Scripting.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Scripting;

public sealed class MongoScriptLogRepository(IMongoDatabase database) : MongoRepositoryBase<ScriptLogRecord>(database), IScriptLogRepository
{
    protected override string CollectionName()
    {
        return "ScriptLogs";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<ScriptLogRecord> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<ScriptLogRecord>(
                Index
                    .Ascending(x => x.AppId)
                    .Descending(x => x.Timestamp)),
            new CreateIndexModel<ScriptLogRecord>(
                Index
                    .Ascending(x => x.Timestamp)),
        ], ct);
    }

    public Task InsertManyAsync(IEnumerable<ScriptLogRecord> records,
        CancellationToken ct = default)
    {
        Guard.NotNull(records);

        var entities = records.ToList();

        if (entities.Count == 0)
        {
            return Task.CompletedTask;
        }

        return Collection.InsertManyAsync(entities, InsertUnordered, ct);
    }

    public async Task TrimAsync(DomainId appId, int maxCount,
        CancellationToken ct = default)
    {
        // Find the newest record that exceeds the limit. Everything that is not newer is deleted.
        var boundary =
            await Collection.Find(x => x.AppId == appId)
                .SortByDescending(x => x.Timestamp)
                .Skip(maxCount)
                .Limit(1)
                .Project(x => x.Timestamp)
                .ToListAsync(ct);

        if (boundary.Count == 0)
        {
            return;
        }

        var timestamp = boundary[0];

        await Collection.DeleteManyAsync(x => x.AppId == appId && x.Timestamp <= timestamp, ct);
    }

    public Task DeleteAsync(DomainId appId,
        CancellationToken ct = default)
    {
        return Collection.DeleteManyAsync(x => x.AppId == appId, ct);
    }

    public Task DeleteOlderThanAsync(Instant timestamp,
        CancellationToken ct = default)
    {
        return Collection.DeleteManyAsync(x => x.Timestamp < timestamp, ct);
    }

    public async Task<IReadOnlyList<ScriptLogRecord>> QueryAsync(DomainId appId, string? name, int skip, int take,
        CancellationToken ct = default)
    {
        var filter = Filter.Eq(x => x.AppId, appId);

        if (!string.IsNullOrWhiteSpace(name))
        {
            // The name is a path, therefore the filter matches all logs in the same group.
            filter &= Filter.Regex(x => x.Name, new BsonRegularExpression($"^{Regex.Escape(name)}"));
        }

        var result =
            await Collection.Find(filter)
                .SortByDescending(x => x.Timestamp)
                .Skip(skip)
                .Limit(take)
                .ToListAsync(ct);

        return result;
    }
}
