// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

#pragma warning disable MA0073 // Avoid comparison with bool constant

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

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

    public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q,
        CancellationToken ct)
    {
        var filter = CreateFilter(app.Id, schemas.Select(x => x.Id), q.ScheduledFrom!.Value, q.ScheduledTo!.Value);

        var contentEntities = await Collection.Find(filter).Limit(100).ToListAsync(ct);
        var contentTotal = (long)contentEntities.Count;

        return ResultList.Create(contentTotal, contentEntities);
    }

    public IAsyncEnumerable<IContentEntity> QueryAsync(Instant now,
        CancellationToken ct)
    {
        var find = Collection.Find(x => x.ScheduledAt < now && x.IsDeleted != true).Not(x => x.Data);

        return find.ToAsyncEnumerable(ct);
    }

    private static FilterDefinition<MongoContentEntity> CreateFilter(DomainId appId,
        IEnumerable<DomainId> schemaIds,
        Instant scheduledFrom,
        Instant scheduledTo)
    {
        return Filter.And(
            Filter.Gte(x => x.ScheduledAt, scheduledFrom),
            Filter.Lte(x => x.ScheduledAt, scheduledTo),
            Filter.Ne(x => x.IsDeleted, true),
            Filter.Eq(x => x.IndexedAppId, appId),
            Filter.In(x => x.IndexedSchemaId, schemaIds));
    }
}
