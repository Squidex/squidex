// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryReferrers : OperationBase
    {
        public override IEnumerable<CreateIndexModel<MongoContentEntity>> CreateIndexes()
        {
            yield return new CreateIndexModel<MongoContentEntity>(Index
                .Ascending(x => x.ReferencedIds)
                .Ascending(x => x.IndexedAppId)
                .Ascending(x => x.IsDeleted));
        }

        public async Task<bool> CheckExistsAsync(DomainId appId, DomainId contentId,
            CancellationToken ct)
        {
            var filter =
                Filter.And(
                    Filter.AnyEq(x => x.ReferencedIds, contentId),
                    Filter.Eq(x => x.IndexedAppId, appId),
                    Filter.Ne(x => x.IsDeleted, true),
                    Filter.Ne(x => x.Id, contentId));

            var hasReferrerAsync =
                await Collection.Find(filter).Only(x => x.Id)
                    .AnyAsync(ct);

            return hasReferrerAsync;
        }
    }
}
