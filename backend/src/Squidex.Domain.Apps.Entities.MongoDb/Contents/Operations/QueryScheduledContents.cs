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
using NodaTime;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryScheduledContents : OperationBase
    {
        protected override Task PrepareAsync(CancellationToken ct = default)
        {
            var index =
                new CreateIndexModel<MongoContentEntity>(Index
                   .Ascending(x => x.ScheduledAt)
                   .Ascending(x => x.IsDeleted));

            return Collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
        }

        public Task DoAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            Guard.NotNull(callback);

            return Collection.Find(x => x.ScheduledAt < now && x.IsDeleted != true)
                .Not(x => x.DataByIds)
                .ForEachAsync(c =>
                {
                    callback(c);
                });
        }
    }
}
