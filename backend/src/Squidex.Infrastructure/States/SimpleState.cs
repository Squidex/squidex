// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;
using Squidex.Log;

namespace Squidex.Infrastructure.States;

public class SimpleState<T> where T : class, new()
{
    private readonly AsyncLock? lockObject;
    private readonly IPersistence<T> persistence;
    private bool isLoaded;
    private Instant lastWrite;

    public T Value { get; set; } = new T();

    public long Version
    {
        get => persistence.Version;
    }

    public IClock Clock { get; set; } = SystemClock.Instance;

    public SimpleState(IPersistenceFactory<T> persistenceFactory, Type ownerType, string id, bool lockOperations = false)
        : this(persistenceFactory, ownerType, DomainId.Create(id), lockOperations)
    {
    }

    public SimpleState(IPersistenceFactory<T> persistenceFactory, Type ownerType, DomainId id, bool lockOperations = false)
    {
        Guard.NotNull(persistenceFactory);

        persistence = persistenceFactory.WithSnapshots(ownerType, id, (state, version) =>
        {
            Value = state;
        });

        if (lockOperations)
        {
            lockObject = new AsyncLock();
        }
    }

    public async Task LoadAsync(
        CancellationToken ct = default)
    {
        using (await LockAsync())
        {
            await LoadInternalAsync(ct);
        }
    }

    public async Task ClearAsync(
        CancellationToken ct = default)
    {
        using (await LockAsync())
        {
            // Reset state first, in case the deletion fails.
            Value = new T();

            await persistence.DeleteAsync(ct);
        }
    }

    public async Task WriteAsync(int ifNotWrittenWithinMs,
        CancellationToken ct = default)
    {
        using (await LockAsync())
        {
            // Calculate the timestamp once.
            var now = Clock.GetCurrentInstant();

            if (ifNotWrittenWithinMs > 0 && now.Minus(lastWrite).TotalMilliseconds < ifNotWrittenWithinMs)
            {
                return;
            }

            await persistence.WriteSnapshotAsync(Value, ct);

            // Only update the last write property if it is successful.
            lastWrite = now;
        }
    }

    public async Task WriteAsync(
        CancellationToken ct = default)
    {
        using (await LockAsync())
        {
            await persistence.WriteSnapshotAsync(Value, ct);

            // Only update the last write property if it is successful.
            lastWrite = Clock.GetCurrentInstant();
        }
    }

    public async Task WriteEventAsync(Envelope<IEvent> envelope,
        CancellationToken ct = default)
    {
        using (await LockAsync())
        {
            await persistence.WriteEventAsync(envelope, ct);

            // Only update the last write property if it is successful.
            lastWrite = Clock.GetCurrentInstant();
        }
    }

    public Task UpdateAsync(Func<T, bool> updater, int retries = 20,
        CancellationToken ct = default)
    {
        return UpdateAsync(state => (updater(state), None.Value), retries, ct);
    }

    public async Task<TResult> UpdateAsync<TResult>(Func<T, (bool, TResult)> updater, int retries = 20,
        CancellationToken ct = default)
    {
        Guard.GreaterEquals(retries, 1);
        Guard.LessThan(retries, 100);

        using (await LockAsync())
        {
            // Ensure that the state is loaded before we make the update.
            if (!isLoaded)
            {
                await LoadInternalAsync(ct);
            }

            for (var i = 0; i < retries; i++)
            {
                try
                {
                    var (isChanged, result) = updater(Value);

                    // If nothing has been changed, we can avoid the call to the database.
                    if (!isChanged)
                    {
                        return result;
                    }

                    await WriteAsync(ct);
                    return result;
                }
                catch (InconsistentStateException) when (i < retries - 1)
                {
                    await LoadInternalAsync(ct);
                }
            }

            return default!;
        }
    }

    private async Task LoadInternalAsync(
        CancellationToken ct = default)
    {
        await persistence.ReadAsync(ct: ct);

        isLoaded = true;
    }

    private Task<IDisposable> LockAsync()
    {
        if (lockObject != null)
        {
            return lockObject.EnterAsync();
        }

        return Task.FromResult<IDisposable>(NoopDisposable.Instance);
    }
}
