// ==========================================================================
//  DomainObjectBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectBase<TBase, TState> : IDomainObject where TState : IDomainState, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private Guid id;
        private TState state;
        private IPersistence<TState> persistence;

        public long Version
        {
            get { return state.Version; }
        }

        public TState State
        {
            get { return state; }
        }

        protected DomainObjectBase()
        {
            state = new TState();
            state.Version = EtagVersion.Empty;
        }

        public IReadOnlyList<Envelope<IEvent>> GetUncomittedEvents()
        {
            return uncomittedEvents;
        }

        public void ClearUncommittedEvents()
        {
            uncomittedEvents.Clear();
        }

        public Task ActivateAsync(string key, IStore store)
        {
            id = Guid.Parse(key);

            persistence = store.WithSnapshots<TBase, TState>(key, s => state = s);

            return persistence.ReadAsync();
        }

        public void RaiseEvent(IEvent @event)
        {
            RaiseEvent(Envelope.Create(@event));
        }

        public void RaiseEvent<TEvent>(Envelope<TEvent> @event) where TEvent : class, IEvent
        {
            Guard.NotNull(@event, nameof(@event));

            @event.SetAggregateId(id);

            OnRaised(@event.To<IEvent>());

            uncomittedEvents.Add(@event.To<IEvent>());
        }

        public void UpdateState(TState newState)
        {
            state = newState;
        }

        protected virtual void OnRaised(Envelope<IEvent> @event)
        {
        }

        public async Task WriteAsync(ISemanticLog log)
        {
            var events = uncomittedEvents.ToArray();

            if (events.Length > 0)
            {
                state.Version += events.Length;

                await persistence.WriteSnapshotAsync(state);

                try
                {
                    await persistence.WriteEventsAsync(events);
                }
                catch (Exception ex)
                {
                    log.LogFatal(ex, w => w.WriteProperty("action", "writeEvents"));
                }
                finally
                {
                    uncomittedEvents.Clear();
                }
            }
        }
    }
}
