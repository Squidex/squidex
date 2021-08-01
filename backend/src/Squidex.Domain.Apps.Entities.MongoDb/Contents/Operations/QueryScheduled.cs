// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryScheduled : OperationBase
    {
        public override IEnumerable<CreateIndexModel<MongoContentEntity>> CreateIndexes()
        {
            yield return new CreateIndexModel<MongoContentEntity>(Index
                .Ascending(x => x.ScheduledAt)
                .Ascending(x => x.IsDeleted)
                .Ascending(x => x.IndexedAppId)
                .Ascending(x => x.IndexedSchemaId));
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(DomainId appId, List<ISchemaEntity> schemas, Q q,
            CancellationToken ct)
        {
            Guard.NotNull(q, nameof(q));

            if (q.ScheduledFrom == null || q.ScheduledTo == null)
            {
                return ResultList.CreateFrom<IContentEntity>(0);
            }

            var filter = CreateFilter(appId, schemas.Select(x => x.Id), q.ScheduledFrom.Value, q.ScheduledTo.Value);

            var contentEntities = await Collection.Find(filter).ToListAsync(ct);
            var contentTotal = (long)contentEntities.Count;

            if (q.NoTotal)
            {
                contentTotal = -1;
            }
            else if (contentTotal >= q.Query.Take || q.Query.Skip > 0)
            {
                contentTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            return ResultList.Create(contentTotal, contentEntities);
        }

        public Task QueryAsync(Instant now, Func<IContentEntity, Task> callback,
            CancellationToken ct)
        {
            Guard.NotNull(callback, nameof(callback));

            return Collection.Find(x => x.ScheduledAt < now && x.IsDeleted != true).Not(x => x.Data)
                .ForEachAsync(c =>
                {
                    callback(c);
                }, ct);
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(DomainId appId, IEnumerable<DomainId> schemaIds, Instant scheduledFrom, Instant scheduledTo)
        {
            return Filter.And(
                Filter.Gte(x => x.ScheduledAt, scheduledFrom),
                Filter.Lte(x => x.ScheduledAt, scheduledTo),
                Filter.Ne(x => x.IsDeleted, true),
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.In(x => x.IndexedSchemaId, schemaIds));
        }
    }
}
