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
    public abstract class DomainObjectBase<TBase, TState> : IDomainObject where TState : new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private int version = -1;
        private TState state = new TState();
        private IPersistence<TState> persistence;

        public TState State
        {
            get { return state; }
        }

        public int Version
        {
            get { return version; }
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
            persistence = store.WithSnapshots<TBase, TState>(key, s => state = s);

            return persistence.ReadAsync();
        }

        protected void RaiseEvent(IEvent @event)
        {
            RaiseEvent(Envelope.Create(@event));
        }

        protected void RaiseEvent<TEvent>(Envelope<TEvent> @event) where TEvent : class, IEvent
        {
            Guard.NotNull(@event, nameof(@event));

            uncomittedEvents.Add(@event.To<IEvent>());
        }

        public void UpdateState(ICommand command, Action<TState> updater)
        {
            state = CloneState(command, updater);
        }

        protected abstract TState CloneState(ICommand command, Action<TState> updater);

        public async Task WriteAsync(ISemanticLog log)
        {
            await persistence.WriteSnapshotAsync(state);

            try
            {
                await persistence.WriteEventsAsync(uncomittedEvents.ToArray());
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
