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
using Squidex.Infrastructure.MongoDb;

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
                await Collection.Find(x => x.AppId == appId && x.Channel.StartsWith(channelPrefix)).SortByDescending(x => x.Created).Limit(count).ToListAsync();

            return entities.Select(x => (IHistoryEventEntity)new ParsedHistoryEvent(x, MessagesEN.Texts)).ToList();
        }

        protected Task On(AppContributorAssigned @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                const string channel = "settings.contributors";

                x.Setup<AppContributorAssigned>(headers, channel)
                    .AddParameter("Contributor", @event.ContributorId)
                    .AddParameter("Permission", @event.Permission.ToString());
            }, false);
        }

        protected Task On(AppContributorRemoved @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                const string channel = "settings.contributors";

                x.Setup<AppContributorRemoved>(headers, channel)
                    .AddParameter("Contributor", @event.ContributorId);
            }, false);
        }

        protected Task On(AppClientRenamed @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                const string channel = "settings.clients";

                x.Setup<AppClientRenamed>(headers, channel)
                    .AddParameter("Id", @event.Id)
                    .AddParameter("Name", !string.IsNullOrWhiteSpace(@event.Name) ? @event.Name : @event.Id);
            }, false);
        }

        protected Task On(AppClientAttached @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                const string channel = "settings.clients";

                x.Setup<AppClientAttached>(headers, channel)
                    .AddParameter("Id", @event.Id);
            }, false);
        }

        protected Task On(AppClientRevoked @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                const string channel = "settings.clients";

                x.Setup<AppClientRevoked>(headers, channel)
                    .AddParameter("Id", @event.Id);
            }, false);
        }

        protected Task On(AppLanguageAdded @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                const string channel = "settings.languages";

                x.Setup<AppLanguageAdded>(headers, channel)
                    .AddParameter("Language", @event.Language.EnglishName);
            }, false);
        }

        protected Task On(AppLanguageRemoved @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                const string channel = "settings.languages";

                x.Setup<AppLanguageRemoved>(headers, channel)
                    .AddParameter("Language", @event.Language.EnglishName);
            }, false);
        }

        protected Task On(AppMasterLanguageSet @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, x =>
            {
                const string channel = "settings.languages";

                x.Setup<AppMasterLanguageSet>(headers, channel)
                    .AddParameter("Language", @event.Language.EnglishName);
            }, false);
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }
    }
}
