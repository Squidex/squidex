// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas
{
    public sealed class MongoSchemasHash : MongoRepositoryBase<MongoSchemasHashEntity>, ISchemasHash, IEventConsumer, IDeleter
    {
        public int BatchSize
        {
            get => 1000;
        }

        public int BatchDelay
        {
            get => 500;
        }

        public string Name
        {
            get => GetType().Name;
        }

        public string EventsFilter
        {
            get => "^schema-";
        }

        public MongoSchemasHash(IMongoDatabase database, bool setup = false)
            : base(database, setup)
        {
        }

        protected override string CollectionName()
        {
            return "SchemasHash";
        }

        async Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            await Collection.DeleteManyAsync(Filter.Eq(x => x.AppId, app.Id), ct);
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
                                .Set(x => x.Updated, @event.Headers.Timestamp()))
                        {
                            IsUpsert = true
                        });
                }
            }

            if (writes.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.BulkWriteAsync(writes, BulkUnordered);
        }

        public async Task<(Instant Create, string Hash)> GetCurrentHashAsync(IAppEntity app,
            CancellationToken ct = default)
        {
            Guard.NotNull(app, nameof(app));

            var entity = await Collection.Find(x => x.AppId == app.Id).FirstOrDefaultAsync(ct);

            if (entity == null)
            {
                return (default, string.Empty);
            }

            var ids =
                entity.SchemaVersions.Select(x => (x.Key, x.Value))
                    .Union(Enumerable.Repeat((app.Id.ToString(), app.Version), 1));

            var hash = CreateHash(ids);

            return (entity.Updated, hash);
        }

        public ValueTask<string> ComputeHashAsync(IAppEntity app, IEnumerable<ISchemaEntity> schemas,
            CancellationToken ct = default)
        {
            var ids =
                schemas.Select(x => (x.Id.ToString(), x.Version))
                    .Union(Enumerable.Repeat((app.Id.ToString(), app.Version), 1));

            var hash = CreateHash(ids);

            return new ValueTask<string>(hash);
        }

        private static string CreateHash(IEnumerable<(string, long)> ids)
        {
            var sb = DefaultPools.StringBuilder.Get();
            try
            {
                foreach (var (id, version) in ids.OrderBy(x => x.Item1))
                {
                    sb.Append(id);
                    sb.Append(version);
                    sb.Append(';');
                }

                return sb.ToString().ToSha256Base64();
            }
            finally
            {
                DefaultPools.StringBuilder.Return(sb);
            }
        }
    }
}
