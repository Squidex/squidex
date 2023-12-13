// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

internal sealed class QueryReferrers : OperationBase
{
    public override IEnumerable<CreateIndexModel<MongoContentEntity>> CreateIndexes()
    {
        yield return new CreateIndexModel<MongoContentEntity>(Index
            .Ascending(x => x.ReferencedIds)
            .Ascending(x => x.IndexedAppId)
            .Ascending(x => x.IsDeleted));
    }

    public async Task<bool> CheckExistsAsync(App app, DomainId reference,
        CancellationToken ct)
    {
        var filter = BuildFilter(app.Id, reference);

        var hasReferrerAsync =
            await Collection.Find(filter).Only(x => x.Id)
                .AnyAsync(ct);

        return hasReferrerAsync;
    }

    public async IAsyncEnumerable<Content> StreamReferencing(DomainId appId, DomainId reference, int take,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var filter = BuildFilter(appId, reference);

        using (var cursor = await Collection.Find(filter).Limit(take).SelectFields(null).ToCursorAsync(ct))
        {
            while (await cursor.MoveNextAsync(ct))
            {
                foreach (var entity in cursor.Current)
                {
                    yield return entity;
                }
            }
        }
    }

    private static FilterDefinition<MongoContentEntity> BuildFilter(DomainId appId, DomainId reference)
    {
        return Filter.And(
            Filter.AnyEq(x => x.ReferencedIds, reference),
            Filter.Eq(x => x.IndexedAppId, appId),
            Filter.Ne(x => x.IsDeleted, true),
            Filter.Ne(x => x.Id, reference));
    }
}
