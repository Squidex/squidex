// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.History
{
    public class MongoHistoryEventRepository : MongoRepositoryBase<HistoryEvent>, IHistoryEventRepository
    {
        public MongoHistoryEventRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_History";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<HistoryEvent> collection, CancellationToken ct = default)
        {
            return collection.Indexes.CreateManyAsync(
                new[]
                {
                    new CreateIndexModel<HistoryEvent>(
                        Index
                            .Ascending(x => x.AppId)
                            .Ascending(x => x.Channel)
                            .Descending(x => x.Created)
                            .Descending(x => x.Version)),
                    new CreateIndexModel<HistoryEvent>(Index.Ascending(x => x.Created),
                        new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(365) })
                }, ct);
        }

        public async Task<IReadOnlyList<HistoryEvent>> QueryByChannelAsync(Guid appId, string channelPrefix, int count)
        {
            if (!string.IsNullOrWhiteSpace(channelPrefix))
            {
                return await Collection.Find(x => x.AppId == appId && x.Channel == channelPrefix).SortByDescending(x => x.Created).ThenByDescending(x => x.Version).Limit(count).ToListAsync();
            }
            else
            {
                return await Collection.Find(x => x.AppId == appId).SortByDescending(x => x.Created).ThenByDescending(x => x.Version).Limit(count).ToListAsync();
            }
        }

        public Task InsertAsync(HistoryEvent item)
        {
            return Collection.ReplaceOneAsync(x => x.Id == item.Id, item, Upsert);
        }

        public Task RemoveAsync(Guid appId)
        {
            return Collection.DeleteManyAsync(x => x.AppId == appId);
        }
    }
}
