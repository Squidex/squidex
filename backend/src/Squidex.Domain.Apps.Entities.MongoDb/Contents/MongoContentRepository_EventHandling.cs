// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
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

        public bool Handles(StoredEvent @event)
        {
            return @event.Data.Type == typeAssetDeleted || @event.Data.Type == typeContentDeleted;
        }

        public Task On(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case AssetDeleted e:
                    return cleanupReferences.DoAsync(e.AssetId);

                case ContentDeleted e:
                    return cleanupReferences.DoAsync(e.ContentId);
            }

            return TaskHelper.Done;
        }

        Task IEventConsumer.ClearAsync()
        {
            return TaskHelper.Done;
        }
    }
}