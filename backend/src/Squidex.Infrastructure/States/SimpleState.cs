// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public class SimpleState<T> where T : class, new()
    {
        private readonly IPersistence<T> persistence;
        private bool isLoaded;
        private Instant lastWrite;

        public T Value { get; set; } = new T();

        public long Version
        {
            get => persistence.Version;
        }

        public IClock Clock { get; set; } = SystemClock.Instance;

        public SimpleState(IPersistenceFactory<T> persistenceFactory, Type ownerType, string id)
            : this(persistenceFactory, ownerType, DomainId.Create(id))
        {
        }

        public SimpleState(IPersistenceFactory<T> persistenceFactory, Type ownerType, DomainId id)
        {
            Guard.NotNull(persistenceFactory);

            persistence = persistenceFactory.WithSnapshots(ownerType, id, (state, version) =>
            {
                Value = state;
            });
        }

        public async Task LoadAsync(
            CancellationToken ct = default)
        {
            await persistence.ReadAsync(ct: ct);

            isLoaded = true;
        }

        public Task ClearAsync(
            CancellationToken ct = default)
        {
            Value = new T();

            return persistence.DeleteAsync(ct);
        }

        public async Task WriteAsync(int ifNotWrittenWithinMs,
            CancellationToken ct = default)
        {
            var now = Clock.GetCurrentInstant();

            if (ifNotWrittenWithinMs > 0 && now.Minus(lastWrite).TotalMilliseconds < ifNotWrittenWithinMs)
            {
                return;
            }

            await persistence.WriteSnapshotAsync(Value, ct);

            lastWrite = now;
        }

        public async Task WriteAsync(
            CancellationToken ct = default)
        {
            await persistence.WriteSnapshotAsync(Value, ct);

            lastWrite = Clock.GetCurrentInstant();
        }

        public async Task WriteEventAsync(Envelope<IEvent> envelope,
            CancellationToken ct = default)
        {
            await persistence.WriteEventAsync(envelope, ct);

            lastWrite = Clock.GetCurrentInstant();
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

            if (!isLoaded)
            {
                await LoadAsync(ct);
            }

            for (var i = 0; i < retries; i++)
            {
                try
                {
                    var (isChanged, result) = updater(Value);

                    if (!isChanged)
                    {
                        return result;
                    }

                    await WriteAsync(ct);
                    return result;
                }
                catch (InconsistentStateException) when (i < retries - 1)
                {
                    await LoadAsync(ct);
                }
            }

            return default!;
        }
    }
}
