// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using NodaTime;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities
{
    public abstract class DomainObjectState<T> :
        IDomainState<T>,
        IEntity,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion
        where T : class
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public RefToken CreatedBy { get; set; }

        [DataMember]
        public RefToken LastModifiedBy { get; set; }

        [DataMember]
        public Instant Created { get; set; }

        [DataMember]
        public Instant LastModified { get; set; }

        [DataMember]
        public long Version { get; set; } = EtagVersion.Empty;

        public abstract bool ApplyEvent(IEvent @event);

        public T Apply(Envelope<IEvent> @event)
        {
            var payload = (SquidexEvent)@event.Payload;

            var clone = (DomainObjectState<T>)MemberwiseClone();

            if (!clone.ApplyEvent(@event.Payload))
            {
                return (this as T)!;
            }

            var headers = @event.Headers;

            if (clone.Id == default)
            {
                clone.Id = headers.AggregateId();
            }

            if (clone.CreatedBy == null)
            {
                clone.Created = headers.Timestamp();
                clone.CreatedBy = payload.Actor;
            }

            clone.LastModified = headers.Timestamp();
            clone.LastModifiedBy = payload.Actor;

            return (clone as T)!;
        }
    }
}
