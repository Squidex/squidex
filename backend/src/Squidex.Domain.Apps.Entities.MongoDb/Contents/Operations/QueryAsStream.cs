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
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    public sealed class QueryAsStream : OperationBase
    {
        protected override async Task PrepareAsync(CancellationToken ct = default)
        {
            var indexBySchema =
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedAppId)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.IndexedSchemaId));

            await Collection.Indexes.CreateOneAsync(indexBySchema, cancellationToken: ct);
        }

        public async IAsyncEnumerable<IContentEntity> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds)
        {
            var find =
                schemaIds != null ?
                    Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted && schemaIds.Contains(x.IndexedSchemaId)) :
                    Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted);

            using (var cursor = await find.ToCursorAsync())
            {
                while (await cursor.MoveNextAsync())
                {
                    foreach (var entity in cursor.Current)
                    {
                        yield return entity;
                    }
                }
            }
        }
    }
}
