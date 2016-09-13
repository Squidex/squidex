// ==========================================================================
//  DomainObject.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Infrastructure.CQRS
{
    public abstract class DomainObject : IAggregate, IEquatable<IAggregate>
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private readonly Guid id;
        private int version;

        public int Version
        {
            get { return version; }
        }

        public Guid Id
        {
            get { return id; }
        }

        protected DomainObject(Guid id, int version)
        {
            Guard.NotEmpty(id, nameof(id));
            Guard.GreaterEquals(version, 0, nameof(version));

            this.id = id;

            this.version = version;
        }

        protected abstract void ApplyEvent(Envelope<IEvent> @event);

        protected void RaiseEvent<TEvent>(Envelope<TEvent> envelope) where TEvent : class, IEvent
        {
            Guard.NotNull(envelope, nameof(envelope));

            var envelopeToAdd = envelope.To<IEvent>();

            uncomittedEvents.Add(envelopeToAdd);

            ApplyEvent(envelopeToAdd);
        }

        protected void RaiseEvent(IEvent @event)
        {
            RaiseEvent(EnvelopeFactory.ForEvent(@event, this));
        }

        void IAggregate.ApplyEvent(Envelope<IEvent> @event)
        {
            ApplyEvent(@event); version++;
        }

        void IAggregate.ClearUncommittedEvents()
        {
            uncomittedEvents.Clear();
        }

        ICollection<Envelope<IEvent>> IAggregate.GetUncomittedEvents()
        {
            return uncomittedEvents;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IAggregate);
        }

        public bool Equals(IAggregate other)
        {
            return other != null && other.Id.Equals(id);
        }
    }
}
