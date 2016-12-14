// ==========================================================================
//  MongoHistoryEventRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Events.Apps;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Read.History;
using Squidex.Read.History.Repositories;
using Squidex.Store.MongoDb.Utils;
using System.Linq;

namespace Squidex.Store.MongoDb.History
{
    public class MongoHistoryEventRepository : MongoRepositoryBase<MongoHistoryEventEntity>, IHistoryEventRepository, ICatchEventConsumer
    {
        public MongoHistoryEventRepository(IMongoDatabase database) 
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_History";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoHistoryEventEntity> collection)
        {
            return Task.WhenAll(
                collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.AppId)),
                collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.Channel)),
                collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.Created), new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(365) }));
        }

        public async Task<List<IHistoryEventEntity>> QueryEventsByChannel(Guid appId, string channelPrefix, int count)
        {
            var entities =
                await Collection.Find(x => x.AppId == appId && x.Channel.StartsWith(channelPrefix)).Limit(count).ToListAsync();

            return entities.Select(x => (IHistoryEventEntity)new ParsedHistoryEvent(x, MessagesEN.Texts)).ToList();
        }

        protected Task On(AppContributorAssigned @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                var channel = $"Apps.{headers.AggregateId()}.Contributors";

                x.Setup<AppContributorAssigned>(headers, channel)
                    .AddParameter("Contributor", @event.ContributorId)
                    .AddParameter("Permission", @event.Permission.ToString());
            });
        }

        protected Task On(AppContributorRemoved @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                var channel = $"Apps.{headers.AggregateId()}.Contributors";

                x.Setup<AppContributorRemoved>(headers, channel)
                    .AddParameter("Contributor", @event.ContributorId);
            });
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }
    }
}
