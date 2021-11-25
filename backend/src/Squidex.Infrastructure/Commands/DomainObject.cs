// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Infrastructure.Commands
{
    public abstract partial class DomainObject<T> where T : class, IDomainState<T>, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private readonly SnapshotList<T> snapshots = new SnapshotList<T>();
        private readonly IPersistenceFactory<T> factory;
        private readonly ISemanticLog log;
        private IPersistence<T>? persistence;
        private bool isLoaded;
        private DomainId uniqueId;

        public DomainId UniqueId
        {
            get => uniqueId;
        }

        public T Snapshot
        {
            get => snapshots.Current;
        }

        public long Version
        {
            get => snapshots.Version;
        }

        protected int Capacity
        {
            get => snapshots.Capacity;
            set => snapshots.Capacity = value;
        }

        protected DomainObject(IPersistenceFactory<T> factory, ISemanticLog log)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNull(log, nameof(log));

            this.factory = factory;

            this.log = log;
        }

        public async Task<T> GetSnapshotAsync(long version)
        {
            var (result, valid) = snapshots.Get(version);

            if (result == null && valid)
            {
                var snapshot = new T
                {
                    Version = EtagVersion.Empty
                };

                snapshots.Add(snapshot, snapshot.Version, false);

                var allEvents = factory.WithEventSourcing(GetType(), UniqueId, @event =>
                {
                    var newVersion = snapshot.Version + 1;

                    if (snapshots.Contains(newVersion))
                    {
                        return false;
                    }

                    return ApplyEvent(@event, true, snapshot, snapshot.Version, false);
                });

                await allEvents.ReadAsync();

                (result, valid) = snapshots.Get(version);
            }

            return result ?? new T { Version = EtagVersion.Empty };
        }

        public virtual void Setup(DomainId uniqueId)
        {
            this.uniqueId = uniqueId;

            persistence = factory.WithSnapshotsAndEventSourcing(GetType(), UniqueId,
                new HandleSnapshot<T>((snapshot, version) =>
                {
                    snapshot.Version = version;
                    snapshots.Add(snapshot, version, true);
                }),
                @event =>
                {
                    if (@event.Payload is IMigratedStateEvent<T> migratable)
                    {
                        var payload = migratable.Migrate(Snapshot);

                        @event = new Envelope<IEvent>(payload, @event.Headers);
                    }

                    return ApplyEvent(@event, true, Snapshot, Version, true);
                });
        }

        public virtual async Task EnsureLoadedAsync(bool silent = false)
        {
            if (isLoaded)
            {
                return;
            }

            if (silent)
            {
                await ReadAsync();
            }
            else
            {
                var logContext = (id: uniqueId.ToString(), name: GetType().Name);

                using (log.MeasureInformation(logContext, (ctx, w) => w
                    .WriteProperty("action", "ActivateDomainObject")
                    .WriteProperty("domainObjectType", ctx.name)
                    .WriteProperty("domainObjectKey", ctx.id)))
                {
                    await ReadAsync();
                }
            }

            isLoaded = true;
        }

        protected void RaiseEvent(IEvent @event)
        {
            RaiseEvent(Envelope.Create(@event));
        }

        protected virtual void RaiseEvent(Envelope<IEvent> @event)
        {
            Guard.NotNull(@event, nameof(@event));

            @event.SetAggregateId(uniqueId);

            if (ApplyEvent(@event, false, Snapshot, Version, true))
            {
                uncomittedEvents.Add(@event);
            }
        }

        public IReadOnlyList<Envelope<IEvent>> GetUncomittedEvents()
        {
            return uncomittedEvents;
        }

        public void ClearUncommittedEvents()
        {
            uncomittedEvents.Clear();
        }

        private async Task<CommandResult> DeleteCoreAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler) where TCommand : ICommand
        {
            Guard.NotNull(handler, nameof(handler));

            var previousSnapshot = Snapshot;
            var previousVersion = Version;
            try
            {
                var result = (await handler(command)) ?? None.Value;

                var events = uncomittedEvents.ToArray();

                if (events != null)
                {
                    var deletedId = DomainId.Combine(UniqueId, DomainId.Create("deleted"));
                    var deletedStream = factory.WithEventSourcing(GetType(), deletedId, null);

                    await deletedStream.WriteEventsAsync(events);

                    if (persistence != null)
                    {
                        await persistence.DeleteAsync();

                        Setup(uniqueId);
                    }

                    snapshots.Clear();
                }

                return new CommandResult(UniqueId, Version, previousVersion, result);
            }
            catch
            {
                RestorePreviousSnapshot(previousSnapshot, previousVersion);

                throw;
            }
            finally
            {
                ClearUncommittedEvents();
            }
        }

        private async Task<CommandResult> UpsertCoreAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler, bool isCreation = false) where TCommand : ICommand
        {
            Guard.NotNull(handler, nameof(handler));

            var wasDeleted = IsDeleted(Snapshot);

            var previousSnapshot = Snapshot;
            var previousVersion = Version;
            try
            {
                var result = (await handler(command)) ?? None.Value;

                var events = uncomittedEvents.ToArray();

                try
                {
                    await WriteAsync(events);
                }
                catch (InconsistentStateException)
                {
                    await EnsureLoadedAsync(true);

                    if (wasDeleted)
                    {
                        if (CanRecreate() && isCreation)
                        {
                            snapshots.ResetTo(new T(), previousVersion);

                            foreach (var @event in uncomittedEvents)
                            {
                                ApplyEvent(@event, false, Snapshot, Version, true);
                            }

                            await WriteAsync(events);
                        }
                        else
                        {
                            throw new DomainObjectDeletedException(uniqueId.ToString());
                        }
                    }
                    else
                    {
                        RestorePreviousSnapshot(previousSnapshot, previousVersion);

                        throw new DomainObjectConflictException(uniqueId.ToString());
                    }
                }

                isLoaded = true;

                return new CommandResult(UniqueId, Version, previousVersion, result);
            }
            catch
            {
                RestorePreviousSnapshot(previousSnapshot, previousVersion);

                throw;
            }
            finally
            {
                ClearUncommittedEvents();
            }
        }

        protected virtual bool CanAcceptCreation(ICommand command)
        {
            return true;
        }

        protected virtual bool CanAccept(ICommand command)
        {
            return true;
        }

        protected virtual bool IsDeleted(T snapshot)
        {
            return false;
        }

        protected virtual bool CanRecreate()
        {
            return false;
        }

        protected virtual bool CanRecreate(IEvent @event)
        {
            return false;
        }

        private void RestorePreviousSnapshot(T previousSnapshot, long previousVersion)
        {
            snapshots.ResetTo(previousSnapshot, previousVersion);
        }

        private bool ApplyEvent(Envelope<IEvent> @event, bool isLoading, T snapshot, long version, bool clean)
        {
            if (IsDeleted(snapshot))
            {
                if (!CanRecreate(@event.Payload))
                {
                    return false;
                }

                snapshot = new T
                {
                    Version = Version
                };
            }

            var newVersion = version + 1;
            var newSnapshot = Apply(snapshot, @event);

            if (ReferenceEquals(snapshot, newSnapshot) && isLoading)
            {
                newSnapshot = snapshot.Copy();
            }

            var isChanged = !ReferenceEquals(snapshot, newSnapshot);

            if (!ReferenceEquals(newSnapshot, snapshot))
            {
                newSnapshot.Version = newVersion;

                snapshots.Add(newSnapshot, newVersion, clean);
            }

            return isChanged;
        }

        private async Task ReadAsync()
        {
            if (persistence != null)
            {
                await persistence.ReadAsync();

                if (persistence.IsSnapshotStale)
                {
                    try
                    {
                        await persistence.WriteSnapshotAsync(Snapshot);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, w => w
                            .WriteProperty("action", "RepairSnapshot")
                            .WriteProperty("status", "Failed"));
                    }
                }
            }
        }

        private async Task WriteAsync(Envelope<IEvent>[] newEvents)
        {
            if (newEvents.Length > 0 && persistence != null)
            {
                await persistence.WriteEventsAsync(newEvents);
                await persistence.WriteSnapshotAsync(Snapshot);
            }
        }

        public async Task RebuildStateAsync()
        {
            await EnsureLoadedAsync(true);

            if (Version <= EtagVersion.Empty)
            {
                throw new DomainObjectNotFoundException(UniqueId.ToString());
            }

            if (persistence != null)
            {
                await persistence.WriteSnapshotAsync(Snapshot);
            }
        }

        protected virtual T Apply(T snapshot, Envelope<IEvent> @event)
        {
            return snapshot.Apply(@event);
        }

        public abstract Task<CommandResult> ExecuteAsync(IAggregateCommand command);
    }
}
