// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Schemas;

public sealed class MongoSchemasHash(IMongoDatabase database) : MongoRepositoryBase<MongoSchemasHashEntity>(database), ISchemasHash, IEventConsumer, IDeleter
{
    public int BatchSize => 1000;

    public int BatchDelay => 500;

    public StreamFilter EventsFilter { get; } = StreamFilter.Prefix("schema-");

    protected override string CollectionName()
    {
        return "SchemasHash";
    }

    Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.AppId, app.Id), ct);
    }

    public Task On(IEnumerable<Envelope<IEvent>> events)
    {
        var writes = new List<WriteModel<MongoSchemasHashEntity>>();

        foreach (var @event in events)
        {
            if (@event.Headers.Restored())
            {
                continue;
            }

            if (@event.Payload is SchemaEvent schemaEvent)
            {
                writes.Add(
                    new UpdateOneModel<MongoSchemasHashEntity>(
                        Filter.Eq(x => x.AppId, schemaEvent.AppId.Id),
                        Update
                            .Set($"s.{schemaEvent.SchemaId.Id}", @event.Headers.EventStreamNumber())
                            .Set(x => x.Updated, @event.Headers.TimestampAsInstant()))
                    {
                        IsUpsert = true,
                    });
            }
        }

        if (writes.Count == 0)
        {
            return Task.CompletedTask;
        }

        return Collection.BulkWriteAsync(writes, BulkUnordered);
    }

    public async Task<SchemasHashKey> GetCurrentHashAsync(App app,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);

        var entity = await Collection.Find(x => x.AppId == app.Id).FirstOrDefaultAsync(ct);
        if (entity == null)
        {
            return SchemasHashKey.Empty;
        }

        return SchemasHashKey.Create(
            app,
            entity.SchemaVersions.ToDictionary(x => DomainId.Create(x.Key), x => x.Value),
            entity.Updated);
    }
}
