// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

internal sealed class QueryReferences : OperationBase
{
    private readonly QueryByIds queryByIds;

    public sealed class ReferencedIdsOnly
    {
        [BsonId]
        [BsonElement("_id")]
        public DomainId DocumentId { get; set; }

        [BsonRequired]
        [BsonElement("rf")]
        public HashSet<DomainId>? ReferencedIds { get; set; }
    }

    public QueryReferences(QueryByIds queryByIds)
    {
        this.queryByIds = queryByIds;
    }

    public async Task<IResultList<Content>> QueryAsync(App app, List<Schema> schemas, Q q,
        CancellationToken ct)
    {
        var find =
            Collection
                .Find(Filter.Eq(x => x.DocumentId, DomainId.Combine(app.Id, q.Referencing)))
                .Project<ReferencedIdsOnly>(Projection.Include(x => x.ReferencedIds));

        var contentEntity = await find.FirstOrDefaultAsync(ct)
            ?? throw new DomainObjectNotFoundException(q.Referencing.ToString());

        if (contentEntity.ReferencedIds is not { Count: > 0 })
        {
            return ResultList.Empty<Content>();
        }

        q = q.WithReferencing(default).WithIds(contentEntity.ReferencedIds!);

        return await queryByIds.QueryAsync(app, schemas, q, ct);
    }
}
