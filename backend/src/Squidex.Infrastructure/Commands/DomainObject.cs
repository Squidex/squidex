// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

#pragma warning disable MA0056 // Do not call overridable members in constructor
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Infrastructure.Commands;

public abstract partial class DomainObject<T> : IAggregate where T : class, IDomainState<T>, new()
{
    private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
    private readonly ILogger log;
    private readonly IPersistenceFactory<T> persistenceFactory;
    private readonly IPersistence<T> persistence;
    private readonly DomainId uniqueId;
    private T snapshot = new T { Version = EtagVersion.Empty };
    private bool isLoaded;

    public DomainId UniqueId
    {
        get => uniqueId;
    }

    public virtual T Snapshot
    {
        get => snapshot;
    }

    public virtual long Version
    {
        get => snapshot.Version;
    }

    protected DomainObject(DomainId uniqueId, IPersistenceFactory<T> persistenceFactory,
        ILogger log)
    {
        Guard.NotNull(persistenceFactory);
        Guard.NotNull(log);

        this.uniqueId = uniqueId;

        this.persistenceFactory = persistenceFactory;

        persistence = persistenceFactory.WithSnapshotsAndEventSourcing(GetType(), UniqueId,
            new HandleSnapshot<T>((newSnapshot, version) =>
            {
                newSnapshot.Version = version;

                snapshot = newSnapshot;
            }),
            @event =>
            {
                var (newSnapshot, changed) = ApplyEvent(@event, Snapshot, Version, false, true);

                if (changed && newSnapshot != null)
                {
                    snapshot = newSnapshot;
                }

                return true;
            });

        this.log = log;
    }

    public virtual async Task<T> GetSnapshotAsync(long version,
        CancellationToken ct = default)
    {
        if (version <= EtagVersion.Any)
        {
            await EnsureLoadedAsync(ct);
            return Snapshot;
        }

        var result = new T
        {
            Version = EtagVersion.Empty
        };

        if (version == result.Version)
        {
            return result;
        }

        var allEvents = persistenceFactory.WithEventSourcing(GetType(), UniqueId, @event =>
        {
            var (newSnapshot, _) = ApplyEvent(@event, result, result.Version, false, false);

            // Can only be null in case of errors or inconsistent streams.
            if (newSnapshot != null && newSnapshot.Version <= version)
            {
                result = newSnapshot;
            }

            return result.Version <= version;
        });

        await allEvents.ReadAsync(ct: ct);

        if (result.Version != version)
        {
            result = new T { Version = EtagVersion.Empty };
        }

        return result;
    }

    public virtual async Task EnsureLoadedAsync(
        CancellationToken ct = default)
    {
        if (isLoaded)
        {
            return;
        }

        await persistence.ReadAsync(ct: ct);

        if (persistence.IsSnapshotStale)
        {
            try
            {
                await persistence.WriteSnapshotAsync(Snapshot, default);
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

        if (ApplyEvent(@event, Snapshot, Version, false, true).Success)
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

    private async Task<CommandResult> DeleteCoreAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task<object?>> handler,
        CancellationToken ct = default) where TCommand : ICommand
    {
        Guard.NotNull(handler);

        var previousSnapshot = Snapshot;
        var previousVersion = Version;
        try
        {
            var result = (await handler(command, ct)) ?? None.Value;

            if (uncomittedEvents.Count > 0)
            {
                var deletedId = DomainId.Combine(UniqueId, DomainId.Create("deleted"));
                var deletedStream = persistenceFactory.WithEventSourcing(GetType(), deletedId, null);

                // Write to the deleted stream first so we never loose this information.
                await deletedStream.WriteEventsAsync(uncomittedEvents, ct);
            }

            // Cleanup the primary stream second.
            await persistence.DeleteAsync(ct);

            snapshot = new T
            {
                Version = EtagVersion.Empty
            };

            return new CommandResult(UniqueId, Version, previousVersion, result);
        }
        catch
        {
            snapshot = previousSnapshot;
            throw;
        }
        finally
        {
            ClearUncommittedEvents();
        }
    }

    private async Task<CommandResult> UpsertCoreAsync<TCommand>(TCommand command, Func<TCommand, CancellationToken, Task<object?>> handler, bool isCreation,
        CancellationToken ct) where TCommand : ICommand
    {
        Guard.NotNull(handler);

        var previousSnapshot = Snapshot;
        var previousVersion = Version;
        try
        {
            var result = (await handler(command, ct)) ?? None.Value;

            var events = uncomittedEvents.ToArray();
            try
            {
                await WriteAsync(events, ct);
            }
            catch (InconsistentStateException ex)
            {
                // Start from the previous, unchanged snapshot.
                snapshot = previousSnapshot;

                // Create commands do not load the domain object for performance reasons, therefore we ensure it is loaded.
                await EnsureLoadedAsync(ct);

                var isDeleted = IsDeleted(Snapshot);

                if (isDeleted && isCreation && CanRecreate())
                {
                    foreach (var @event in uncomittedEvents)
                    {
                        ApplyEvent(@event, Snapshot, Version, false, true);
                    }

                    await WriteAsync(events, ct);
                }
                else if (isDeleted)
                {
                    throw new DomainObjectDeletedException(uniqueId.ToString());
                }
                else if (isCreation)
                {
                    throw new DomainObjectConflictException(uniqueId.ToString());
                }
                else
                {
                    throw new DomainObjectVersionException(uniqueId.ToString(), ex.VersionCurrent, ex.VersionExpected, ex);
                }
            }

            isLoaded = true;

            return new CommandResult(UniqueId, Version, previousVersion, result);
        }
        catch
        {
            snapshot = previousSnapshot;
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

    private (T?, bool Success) ApplyEvent(Envelope<IEvent> @event, T snapshot, long version, bool loading, bool update)
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

        @event = @event.Migrate(snapshot);

        var newVersion = version + 1;
        var newSnapshot = Apply(snapshot, @event);

        var isChanged = loading || !ReferenceEquals(snapshot, newSnapshot);

        // If we are loading events at the moment, we will always update the version.
        if (isChanged)
        {
            newSnapshot.Version = newVersion;
        }

        if (update)
        {
            this.snapshot = newSnapshot;
        }

        return (newSnapshot, isChanged);
    }

    private async Task WriteAsync(Envelope<IEvent>[] newEvents,
        CancellationToken ct)
    {
        if (newEvents.Length > 0)
        {
            // Writing the events is the first step, so we can cancel it if requested by the user, but if events are written,
            // we should also write the snapshots to keep both collection / database as consitent as possible.
            await persistence.WriteEventsAsync(newEvents, ct);
            await persistence.WriteSnapshotAsync(Snapshot, default);
        }
    }

    public async Task RebuildStateAsync(
        CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);

        if (Version <= EtagVersion.Empty)
        {
            throw new DomainObjectNotFoundException(UniqueId.ToString());
        }

        await persistence.WriteSnapshotAsync(Snapshot, ct);
    }

    protected virtual T Apply(T snapshot, Envelope<IEvent> @event)
    {
        return snapshot.Apply(@event);
    }

    public abstract Task<CommandResult> ExecuteAsync(IAggregateCommand command,
        CancellationToken ct);
}
