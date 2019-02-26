// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class TextIndexer : IEventConsumer
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public Task On(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case ContentCreated contentCreated:
                    break;
                case ContentUpdated contentUpdated:
                    break;
                case ContentUpdateProposed contentUpdateProposed:
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
