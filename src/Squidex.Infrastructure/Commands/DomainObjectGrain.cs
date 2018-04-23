// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectGrain<T> : GrainOfGuid, IDomainObjectGrain where T : IDomainState, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private readonly IStore<Guid> store;
        private Guid id;
        private T snapshot = new T { Version = EtagVersion.Empty };
        private IPersistence<T> persistence;

        public Guid Id
        {
            get { return id; }
        }

        public long Version
        {
            get { return snapshot.Version; }
        }

        public long NewVersion
        {
            get { return snapshot.Version + uncomittedEvents.Count; }
        }

        public T Snapshot
        {
            get { return snapshot; }
        }

        protected DomainObjectGrain(IStore<Guid> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public override Task OnActivateAsync(Guid key)
        {
            id = key;

            persistence = store.WithSnapshotsAndEventSourcing<T, Guid>(GetType(), id, ApplySnapshot, ApplyEvent);

            return persistence.ReadAsync();
        }

        public void RaiseEvent(IEvent @event)
        {
            RaiseEvent(Envelope.Create(@event));
        }

        public virtual void RaiseEvent(Envelope<IEvent> @event)
        {
            Guard.NotNull(@event, nameof(@event));

            @event.SetAggregateId(id);

            ApplyEvent(@event);

            uncomittedEvents.Add(@event);
        }

        public IReadOnlyList<Envelope<IEvent>> GetUncomittedEvents()
        {
            return uncomittedEvents;
        }

        public void ClearUncommittedEvents()
        {
            uncomittedEvents.Clear();
        }

        public virtual void ApplySnapshot(T newSnapshot)
        {
            snapshot = newSnapshot;
        }

        public virtual void ApplyEvent(Envelope<IEvent> @event)
        {
        }

        public Task WriteSnapshotAsync()
        {
            snapshot.Version = persistence.Version;

            return persistence.WriteSnapshotAsync(snapshot);
        }

        protected Task<object> CreateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object>> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler, false);
        }

        protected Task<object> CreateReturnAsync<TCommand>(TCommand command, Func<TCommand, object> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToAsync(), false);
        }

        protected Task<object> CreateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler.ToDefault<TCommand, object>(), false);
        }

        protected Task<object> CreateAsync<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToDefault<TCommand, object>()?.ToAsync(), false);
        }

        protected Task<object> UpdateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object>> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler, true);
        }

        protected Task<object> UpdateReturnAsync<TCommand>(TCommand command, Func<TCommand, object> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToAsync(), true);
        }

        protected Task<object> UpdateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToDefault<TCommand, object>(), true);
        }

        protected Task<object> UpdateAsync<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : class, IAggregateCommand
        {
            return InvokeAsync(command, handler?.ToDefault<TCommand, object>()?.ToAsync(), true);
        }

        private async Task<object> InvokeAsync<TCommand>(TCommand command, Func<TCommand, Task<object>> handler, bool isUpdate) where TCommand : class, IAggregateCommand
        {
            Guard.NotNull(command, nameof(command));

            if (command.ExpectedVersion != EtagVersion.Any && command.ExpectedVersion != Version)
            {
                throw new DomainObjectVersionException(id.ToString(), GetType(), Version, command.ExpectedVersion);
            }

            if (isUpdate && Version < 0)
            {
                try
                {
                    DeactivateOnIdle();
                }
                catch (InvalidOperationException)
                {
                }

                throw new DomainObjectNotFoundException(id.ToString(), GetType());
            }

            if (!isUpdate && Version >= 0)
            {
                throw new DomainException("Object has already been created.");
            }

            var previousSnapshot = snapshot;
            try
            {
                var result = await handler(command);

                var events = uncomittedEvents.ToArray();

                if (events.Length > 0)
                {
                    snapshot.Version = NewVersion;

                    await persistence.WriteEventsAsync(events);
                    await persistence.WriteSnapshotAsync(snapshot);
                }

                if (result == null)
                {
                    if (isUpdate)
                    {
                        result = new EntitySavedResult(Version);
                    }
                    else
                    {
                        result = EntityCreatedResult.Create(id, Version);
                    }
                }

                return result;
            }
            catch
            {
                snapshot = previousSnapshot;

                throw;
            }
            finally
            {
                uncomittedEvents.Clear();
            }
        }

        public async Task<J<object>> ExecuteAsync(J<IAggregateCommand> command)
        {
            var result = await ExecuteAsync(command.Value);

            return result.AsJ();
        }

        protected abstract Task<object> ExecuteAsync(IAggregateCommand command);
    }
}
