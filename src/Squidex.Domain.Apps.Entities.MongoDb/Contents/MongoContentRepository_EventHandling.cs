// ==========================================================================
//  MongoContentRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^(content-)|(asset-)"; }
        }

        public override Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
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
    }
}