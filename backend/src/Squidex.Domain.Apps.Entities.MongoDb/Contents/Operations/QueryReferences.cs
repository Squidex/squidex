// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryReferences : OperationBase
    {
        private static readonly IResultList<IContentEntity> EmptyIds = ResultList.CreateFrom<IContentEntity>(0);
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

        public async Task<IResultList<IContentEntity>> QueryAsync(DomainId appId, List<ISchemaEntity> schemas, Q q,
            CancellationToken ct)
        {
            var documentId = DomainId.Combine(appId, q.Referencing);

            var find =
                Collection
                    .Find(x => x.DocumentId == documentId)
                    .Project<ReferencedIdsOnly>(Projection.Include(x => x.ReferencedIds));

            var contentEntity = await find.FirstOrDefaultAsync(ct);

            if (contentEntity == null)
            {
                throw new DomainObjectNotFoundException(q.Referencing.ToString());
            }

            if (contentEntity.ReferencedIds == null || contentEntity.ReferencedIds.Count == 0)
            {
                return EmptyIds;
            }

            q = q.WithReferencing(default).WithIds(contentEntity.ReferencedIds!);

            return await queryByIds.QueryAsync(appId, schemas, q, ct);
        }
    }
}
