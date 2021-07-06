// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Entities.Contents;
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
                .Ascending(x => x.IsDeleted));
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
    }
}
