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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectGrain<T> : GrainOfGuid, IDomainObjectGrain where T : IDomainState, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private readonly IStore<Guid> store;
        private readonly ISemanticLog log;
        private readonly List<T> snapshots = new List<T> { new T { Version = EtagVersion.Empty } };
        private bool cleanup;
        private Guid id;
        private IPersistence<T> persistence;

        public Guid Id
        {
            get { return id; }
        }

        public long Version
        {
            get { return snapshots.Count - 2; }
        }

        public long NewVersion
        {
            get { return Version; }
        }

        public T Snapshot
        {
            get { return snapshots[snapshots.Count - 1]; }
        }

        protected DomainObjectGrain(IStore<Guid> store, ISemanticLog log)
        {
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(log, nameof(log));

            this.store = store;

            this.log = log;
        }

        public void CleanupOldSnapshots()
        {
            cleanup = true;
        }

        public T GetSnapshot(long version)
        {
            if (version == EtagVersion.Any)
            {
                return Snapshot;
            }

            if (version == EtagVersion.Empty)
            {
                return snapshots[0];
            }

            if (version >= 0 && version < snapshots.Count - 1)
            {
                return snapshots[(int)version + 1];
            }

            return default(T);
        }

        public override async Task OnActivateAsync(Guid key)
        {
            using (log.MeasureInformation(w => w
                .WriteProperty("action", "ActivateDomainObject")
                .WriteProperty("domainObjectType", GetType().Name)
                .WriteProperty("domainObjectKey", key.ToString())))
            {
                id = key;

                persistence = store.WithSnapshotsAndEventSourcing<T, Guid>(GetType(), id, ApplySnapshot, ApplyEvent);

                await persistence.ReadAsync();
            }
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

        public virtual void ApplySnapshot(T snapshot)
        {
            snapshot.Version = snapshots.Count - 1;

            snapshots.Add(snapshot);
        }

        public virtual void ApplyEvent(Envelope<IEvent> @event)
        {
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

            var size = snapshots.Count;

            try
            {
                var result = await handler(command);

                var events = uncomittedEvents.ToArray();

                if (events.Length > 0)
                {
                    await persistence.WriteEventsAsync(events);
                    await persistence.WriteSnapshotAsync(Snapshot);
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
                while (snapshots.Count > size)
                {
                    snapshots.RemoveAt(snapshots.Count - 1);
                }

                throw;
            }
            finally
            {
                if (cleanup)
                {
                    for (var i = 0; i < snapshots.Count - 1; i++)
                    {
                        snapshots[i] = default(T);
                    }
                }

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
