// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public abstract partial class DomainObject<T> where T : class, IDomainState<T>, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private readonly SnapshotList<T> snapshots = new SnapshotList<T>();
        private readonly ILogger log;
        private readonly IPersistenceFactory<T> factory;
        private readonly IPersistence<T> persistence;
        private readonly DomainId uniqueId;
        private bool isLoaded;

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

        protected DomainObject(DomainId uniqueId, IPersistenceFactory<T> factory,
            ILogger log)
        {
            Guard.NotNull(factory);
            Guard.NotNull(log);

            this.uniqueId = uniqueId;
            this.factory = factory;

            persistence = factory.WithSnapshotsAndEventSourcing(GetType(), UniqueId,
                new HandleSnapshot<T>((snapshot, version) =>
                {
                    snapshot.Version = version;
                    snapshots.Add(snapshot, version, true);
                }),
                @event =>
                {
                    // Some migrations needs the current state.
                    @event = @event.Migrate(Snapshot);

                    return ApplyEvent(@event, true, Snapshot, Version, true).Success;
                });

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
                    // Some migrations needs the current state.
                    @event = @event.Migrate(snapshot);

                    var (newSnapshot, isChanged) = ApplyEvent(@event, true, snapshot, snapshot.Version, false);

                    // Can only be null in case of errors or inconsistent streams.
                    if (newSnapshot != null)
                    {
                        snapshot = newSnapshot;
                    }

                    // If all snapshorts from this one here are valid we can stop.
                    return newSnapshot != null && !snapshots.ContainsThisAndNewer(newSnapshot.Version);
                });

                await allEvents.ReadAsync();

                (result, valid) = snapshots.Get(version);
            }

            return result ?? new T { Version = EtagVersion.Empty };
        }

        public virtual async Task EnsureLoadedAsync()
        {
            if (isLoaded)
            {
                return;
            }

            await persistence.ReadAsync();

            if (persistence.IsSnapshotStale)
            {
                try
                {
                    await persistence.WriteSnapshotAsync(Snapshot);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to repair snapshot for domain object of type {type} with ID {id}.", GetType(), UniqueId);
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

            if (ApplyEvent(@event, false, Snapshot, Version, true).Success)
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
            Guard.NotNull(handler);

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

                    // Write to the deleted stream first so we never loose this information.
                    await deletedStream.WriteEventsAsync(events);

                    // Cleanup the secondary stream first.
                    await persistence.DeleteAsync();

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
            Guard.NotNull(handler);

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
                    await EnsureLoadedAsync();

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

        private (T?, bool Success) ApplyEvent(Envelope<IEvent> @event, bool isLoading, T snapshot, long version, bool clean)
        {
            if (IsDeleted(snapshot))
            {
                if (!CanRecreate(@event.Payload))
                {
                    return default;
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

            if (isChanged)
            {
                newSnapshot.Version = newVersion;

                snapshots.Add(newSnapshot, newVersion, clean);
            }

            return (newSnapshot, isChanged);
        }

        private async Task WriteAsync(Envelope<IEvent>[] newEvents)
        {
            if (newEvents.Length == 0)
            {
                return;
            }

            await persistence.WriteEventsAsync(newEvents);
            await persistence.WriteSnapshotAsync(Snapshot);
        }

        public async Task RebuildStateAsync()
        {
            await EnsureLoadedAsync();

            if (Version <= EtagVersion.Empty)
            {
                throw new DomainObjectNotFoundException(UniqueId.ToString());
            }

            await persistence.WriteSnapshotAsync(Snapshot);
        }

        protected virtual T Apply(T snapshot, Envelope<IEvent> @event)
        {
            return snapshot.Apply(@event);
        }

        public abstract Task<CommandResult> ExecuteAsync(IAggregateCommand command);
    }
}
