// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : IEventConsumer
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^(content-)|(asset-)"; }
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload);
        }

        protected Task On(AssetDeleted @event)
        {
            return Collection.UpdateManyAsync(
                Filter.And(
                    Filter.AnyEq(x => x.ReferencedIds, @event.AssetId),
                    Filter.AnyNe(x => x.ReferencedIdsDeleted, @event.AssetId)),
                Update.AddToSet(x => x.ReferencedIdsDeleted, @event.AssetId));
        }

        protected Task On(ContentDeleted @event)
        {
            return Collection.UpdateManyAsync(
                Filter.And(
                    Filter.AnyEq(x => x.ReferencedIds, @event.ContentId),
                    Filter.AnyNe(x => x.ReferencedIdsDeleted, @event.ContentId)),
                Update.AddToSet(x => x.ReferencedIdsDeleted, @event.ContentId));
        }

        Task IEventConsumer.ClearAsync()
        {
            return TaskHelper.Done;
        }
    }
}