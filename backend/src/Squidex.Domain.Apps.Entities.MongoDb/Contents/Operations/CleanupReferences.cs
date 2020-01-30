// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class CleanupReferences : OperationBase
    {
        protected override Task PrepareAsync(CancellationToken ct = default)
        {
            var index =
                new CreateIndexModel<MongoContentEntity>(
                    Index.Ascending(x => x.ReferencedIds));

            return Collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
        }

        public Task DoAsync(Guid id)
        {
            return Collection.UpdateManyAsync(
                Filter.And(
                    Filter.AnyEq(x => x.ReferencedIds, id),
                    Filter.AnyNe(x => x.ReferencedIdsDeleted, id)),
                Update.AddToSet(x => x.ReferencedIdsDeleted, id));
        }
    }
}
