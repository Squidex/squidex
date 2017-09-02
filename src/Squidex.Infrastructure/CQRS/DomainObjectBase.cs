// ==========================================================================
//  DomainObjectBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure.CQRS.Events;

// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace Squidex.Infrastructure.CQRS
{
    public abstract class DomainObjectBase : IAggregate, IEquatable<IAggregate>
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

        protected DomainObjectBase(Guid id, int version)
        {
            Guard.NotEmpty(id, nameof(id));
            Guard.GreaterEquals(version, -1, nameof(version));

            this.id = id;

            this.version = version;
        }

        protected abstract void DispatchEvent(Envelope<IEvent> @event);

        private void ApplyEventCore(Envelope<IEvent> @event)
        {
            DispatchEvent(@event); version++;
        }

        protected void RaiseEvent(IEvent @event)
        {
            RaiseEvent(Envelope.Create(@event));
        }

        protected void RaiseEvent<TEvent>(Envelope<TEvent> @event) where TEvent : class, IEvent
        {
            Guard.NotNull(@event, nameof(@event));

            uncomittedEvents.Add(@event.To<IEvent>());

            ApplyEventCore(@event.To<IEvent>());
        }

        void IAggregate.ApplyEvent(Envelope<IEvent> @event)
        {
            ApplyEventCore(@event);
        }

        void IAggregate.ClearUncommittedEvents()
        {
            uncomittedEvents.Clear();
        }

        public ICollection<Envelope<IEvent>> GetUncomittedEvents()
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
