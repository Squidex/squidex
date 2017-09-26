// ==========================================================================
//  ContentVersionLoader.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Write.Contents
{
    public sealed class ContentVersionLoader : IContentVersionLoader
    {
        private readonly IStreamNameResolver nameResolver;
        private readonly IEventStore eventStore;
        private readonly EventDataFormatter formatter;

        public ContentVersionLoader(IEventStore eventStore, IStreamNameResolver nameResolver, EventDataFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(nameResolver, nameof(nameResolver));

            this.formatter = formatter;
            this.eventStore = eventStore;
            this.nameResolver = nameResolver;
        }

        public async Task<NamedContentData> LoadAsync(Guid appId, Guid id, long version)
        {
            var streamName = nameResolver.GetStreamName(typeof(ContentDomainObject), id);

            var events = await eventStore.GetEventsAsync(streamName);

            if (events.Count == 0 || events.Count < version - 1)
            {
                throw new DomainObjectNotFoundException(id.ToString(), typeof(ContentDomainObject));
            }

            NamedContentData contentData = null;

            foreach (var storedEvent in events.Where(x => x.EventStreamNumber <= version))
            {
                var envelope = ParseKnownEvent(storedEvent);

                if (envelope != null)
                {
                    if (envelope.Payload is ContentCreated contentCreated)
                    {
                        if (contentCreated.AppId.Id != appId)
                        {
                            throw new DomainObjectNotFoundException(id.ToString(), typeof(ContentDomainObject));
                        }

                        contentData = contentCreated.Data;
                    }
                    else if (envelope.Payload is ContentUpdated contentUpdated)
                    {
                        contentData = contentUpdated.Data;
                    }
                }
            }

            return contentData;
        }

        private Envelope<IEvent> ParseKnownEvent(StoredEvent storedEvent)
        {
            try
            {
                return formatter.Parse(storedEvent.Data);
            }
            catch (TypeNameNotFoundException)
            {
                return null;
            }
        }
    }
}
