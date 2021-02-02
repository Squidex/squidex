// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
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
    public sealed class MongoSchemasHash : MongoRepositoryBase<MongoSchemasHashEntity>, ISchemasHash, IEventConsumer
    {
        public int BatchSize
        {
            get { return 1000; }
        }

        public int BatchDelay
        {
            get { return 500; }
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^(app-|schema-)"; }
        }

        public MongoSchemasHash(IMongoDatabase database, bool setup = false)
            : base(database, setup)
        {
        }

        protected override string CollectionName()
        {
            return "SchemasHash";
        }

        public Task On(IEnumerable<Envelope<IEvent>> events)
        {
            var writes = new List<WriteModel<MongoSchemasHashEntity>>();

            foreach (var @event in events)
            {
                switch (@event.Payload)
                {
                    case SchemaEvent schemaEvent:
                        {
                            writes.Add(
                                new UpdateOneModel<MongoSchemasHashEntity>(
                                    Filter.Eq(x => x.AppId, schemaEvent.AppId.Id.ToString()),
                                    Update
                                        .Set($"s.{schemaEvent.SchemaId.Id}", @event.Headers.EventStreamNumber())
                                        .Set(x => x.Updated, @event.Headers.Timestamp())));
                            break;
                        }

                    case AppEvent appEvent:
                        writes.Add(
                            new UpdateOneModel<MongoSchemasHashEntity>(
                                Filter.Eq(x => x.AppId, appEvent.AppId.Id.ToString()),
                                Update
                                    .Set(x => x.AppVersion, @event.Headers.EventStreamNumber())
                                    .Set(x => x.Updated, @event.Headers.Timestamp()))
                            {
                                IsUpsert = true
                            });
                        break;
                }
            }

            if (writes.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.BulkWriteAsync(writes);
        }

        public async Task<(Instant Create, string Hash)> GetCurrentHashAsync(DomainId appId)
        {
            var entity = await Collection.Find(x => x.AppId == appId.ToString()).FirstOrDefaultAsync();

            if (entity == null)
            {
                return (default, string.Empty);
            }

            var ids =
                entity.SchemaVersions.Select(x => (x.Key, x.Value))
                    .Union(Enumerable.Repeat((entity.AppId, entity.AppVersion), 1));

            var hash = CreateHash(ids);

            return (entity.Updated, hash);
        }

        public ValueTask<string> ComputeHashAsync(IAppEntity app, IEnumerable<ISchemaEntity> schemas)
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

                return sb.ToString().Sha256Base64();
            }
            finally
            {
                DefaultPools.StringBuilder.Return(sb);
            }
        }
    }
}
