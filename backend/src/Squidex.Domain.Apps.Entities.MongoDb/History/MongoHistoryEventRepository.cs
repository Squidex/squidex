﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.History
{
    public class MongoHistoryEventRepository : MongoRepositoryBase<HistoryEvent>, IHistoryEventRepository
    {
        static MongoHistoryEventRepository()
        {
            BsonClassMap.RegisterClassMap<HistoryEvent>(cm =>
            {
                cm.AutoMap();

                cm.MapProperty(x => x.EventType)
                    .SetElementName("Message");
            });
        }

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
            return collection.Indexes.CreateOneAsync(
                new CreateIndexModel<HistoryEvent>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.Channel)
                        .Descending(x => x.Created)
                        .Descending(x => x.Version)),
                cancellationToken: ct);
        }

        public async Task<IReadOnlyList<HistoryEvent>> QueryByChannelAsync(DomainId appId, string channelPrefix, int count)
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
            return Collection.ReplaceOneAsync(x => x.Id == item.Id, item, UpsertReplace);
        }

        public Task RemoveAsync(DomainId appId)
        {
            return Collection.DeleteManyAsync(x => x.AppId == appId);
        }
    }
}
