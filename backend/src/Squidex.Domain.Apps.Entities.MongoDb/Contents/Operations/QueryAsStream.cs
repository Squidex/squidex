// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    public sealed class QueryAsStream : OperationBase
    {
        public override IEnumerable<CreateIndexModel<MongoContentEntity>> CreateIndexes()
        {
            yield return new CreateIndexModel<MongoContentEntity>(Index
                .Ascending(x => x.IndexedAppId)
                .Ascending(x => x.IsDeleted)
                .Ascending(x => x.IndexedSchemaId));
        }

        public async IAsyncEnumerable<IContentEntity> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds,
            [EnumeratorCancellation] CancellationToken ct)
        {
            var find =
                schemaIds != null ?
                    Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted && schemaIds.Contains(x.IndexedSchemaId)) :
                    Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted);

            using (var cursor = await find.ToCursorAsync(ct))
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
    }
}
