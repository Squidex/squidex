// ==========================================================================
//  MongoAssetRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Read.MongoDb.Utils;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Read.MongoDb.Assets
{
    public partial class MongoAssetRepository
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^asset-"; }
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected Task On(AssetCreated @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(@event, headers, a =>
            {
                SimpleMapper.Map(@event, a);
            });
        }

        protected Task On(AssetUpdated @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                SimpleMapper.Map(@event, a);
            });
        }

        protected Task On(AssetRenamed @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                SimpleMapper.Map(@event, a);
            });
        }

        protected Task On(AssetDeleted @event, EnvelopeHeaders headers)
        {
            return Collection.DeleteOneAsync(x => x.Id == @event.AssetId);
        }
    }
}
