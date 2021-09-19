// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities
{
    public abstract class DomainObjectState<T> : IDomainState<T> where T : class
    {
        public DomainId Id { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public long Version { get; set; }

        protected DomainObjectState()
        {
            Version = EtagVersion.Empty;
        }

        public virtual bool ApplyEvent(IEvent @event, EnvelopeHeaders headers)
        {
            return ApplyEvent(@event);
        }

        public virtual bool ApplyEvent(IEvent @event)
        {
            return false;
        }

        public T Apply(Envelope<IEvent> @event)
        {
            var payload = (SquidexEvent)@event.Payload;

            var clone = (DomainObjectState<T>)MemberwiseClone();

            if (!clone.ApplyEvent(@event.Payload, @event.Headers))
            {
                return (this as T)!;
            }

            var headers = @event.Headers;

            if (clone.Id == DomainId.Empty)
            {
                clone.Id = headers.AggregateId();
            }

            var timestamp = headers.Timestamp();

            if (clone.CreatedBy == null)
            {
                clone.Created = timestamp;
                clone.CreatedBy = payload.Actor;
            }

            clone.LastModified = timestamp;
            clone.LastModifiedBy = payload.Actor;

            return (clone as T)!;
        }
    }
}
