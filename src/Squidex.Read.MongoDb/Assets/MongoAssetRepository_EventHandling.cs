using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Squidex.Events.Apps;
using Squidex.Events.Assets;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.MongoDb.Utils;

namespace Squidex.Read.MongoDb.Assets
{
    public partial class MongoAssetRepository
    {
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
            return Collection.DeleteOneAsync(Filter.Eq(x => x.Id, @event.AssetId));
        }
    }
}
