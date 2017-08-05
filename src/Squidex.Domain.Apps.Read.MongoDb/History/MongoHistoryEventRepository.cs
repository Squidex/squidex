// ==========================================================================
//  MongoHistoryEventRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Read.History;
using Squidex.Domain.Apps.Read.History.Repositories;
using Squidex.Domain.Apps.Read.MongoDb.Utils;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.History
{
    public class MongoHistoryEventRepository : MongoRepositoryBase<MongoHistoryEventEntity>, IHistoryEventRepository, IEventConsumer
    {
        private readonly List<IHistoryEventsCreator> creators;
        private readonly Dictionary<string, string> texts = new Dictionary<string, string>();
        private int sessionEventCount;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return ".*"; }
        }

        public MongoHistoryEventRepository(IMongoDatabase database, IEnumerable<IHistoryEventsCreator> creators)
            : base(database)
        {
            this.creators = creators.ToList();

            foreach (var creator in this.creators)
            {
                foreach (var text in creator.Texts)
                {
                    texts[text.Key] = text.Value;
                }
            }
        }

        protected override string CollectionName()
        {
            return "Projections_History";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoHistoryEventEntity> collection)
        {
            return Task.WhenAll(
                collection.Indexes.CreateOneAsync(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.Channel)
                        .Descending(x => x.Created)
                        .Descending(x => x.SessionEventIndex)),
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Created), new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(365) }));
        }

        public async Task<IReadOnlyList<IHistoryEventEntity>> QueryByChannelAsync(Guid appId, string channelPrefix, int count)
        {
            var historyEventEntities =
                await Collection.Find(x => x.AppId == appId && x.Channel == channelPrefix)
                    .SortByDescending(x => x.Created).ThenByDescending(x => x.SessionEventIndex).Limit(count)
                    .ToListAsync();

            return historyEventEntities.Select(x => (IHistoryEventEntity)new ParsedHistoryEvent(x, texts)).ToList();
        }

        public async Task On(Envelope<IEvent> @event)
        {
            foreach (var creator in creators)
            {
                var message = await creator.CreateEventAsync(@event);

                if (message != null)
                {
                    await Collection.CreateAsync((SquidexEvent)@event.Payload, @event.Headers, entity =>
                    {
                        entity.Id = Guid.NewGuid();

                        entity.SessionEventIndex = Interlocked.Increment(ref sessionEventCount);

                        entity.Channel = message.Channel;
                        entity.Message = message.Message;

                        entity.Parameters = message.Parameters.ToDictionary(p => p.Key, p => p.Value);
                    });
                }
            }
        }
    }
}
