// ==========================================================================
//  MongoAppRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Events.Apps;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Repositories;
using Squidex.Store.MongoDb.Utils;
using Squidex.Infrastructure;

namespace Squidex.Store.MongoDb.Apps
{
    public sealed class MongoAppRepository : MongoRepositoryBase<MongoAppEntity>, IAppRepository, ICatchEventConsumer
    {
        public MongoAppRepository(IMongoDatabase database) 
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_Apps";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAppEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.Name));
        }

        public async Task<IReadOnlyList<IAppEntity>> QueryAllAsync(string subjectId)
        {
            var entities =
                await Collection.Find(s => s.Contributors.ContainsKey(subjectId)).ToListAsync();

            return entities;
        }

        public async Task<IAppEntity> FindAppByNameAsync(string name)
        {
            var entity =
                await Collection.Find(s => s.Name == name).FirstOrDefaultAsync();

            return entity;
        }

        public Task On(AppCreated @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(headers, a =>
            {
                SimpleMapper.Map(@event, a);
            });
        }

        public Task On(AppContributorRemoved @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, a =>
            {
                a.Contributors.Remove(@event.ContributorId);
            });
        }

        public Task On(AppLanguagesConfigured @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, a =>
            {
                a.Languages = @event.Languages.Select(x => x.Iso2Code).ToList();
            });
        }

        public Task On(AppClientAttached @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, a =>
            {
                a.Clients.Add(@event.ClientId, SimpleMapper.Map(@event, new MongoAppClientEntity()));
            });
        }

        public Task On(AppClientRevoked @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, a =>
            {
                a.Clients.Remove(@event.ClientId);
            });
        }

        public Task On(AppClientRenamed @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, a =>
            {
                a.Clients[@event.ClientId].Name = @event.Name;
            });
        }

        public Task On(AppContributorAssigned @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(headers, a =>
            {
                var contributor = a.Contributors.GetOrAddNew(@event.ContributorId);

                SimpleMapper.Map(@event, contributor);
            });
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }
    }
}
