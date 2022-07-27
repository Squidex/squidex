// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public class SimpleState<T> where T : class, new()
    {
        private readonly IPersistence<T> persistence;
        private bool isLoaded;

        public T Value { get; set; } = new T();

        public long Version
        {
            get => persistence.Version;
        }

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

        public Task WriteAsync(
            CancellationToken ct = default)
        {
            return persistence.WriteSnapshotAsync(Value, ct);
        }

        public Task WriteEventAsync(Envelope<IEvent> envelope,
            CancellationToken ct = default)
        {
            return persistence.WriteEventAsync(envelope, ct);
        }

        public async Task UpdateAsync(Func<T, bool> updater, int retries = 20,
            CancellationToken ct = default)
        {
            await EnsureLoadedAsync(ct);

            for (var i = 0; i < retries; i++)
            {
                try
                {
                    var isChanged = updater(Value);

                    if (!isChanged)
                    {
                        return;
                    }

                    await WriteAsync(ct);
                    return;
                }
                catch (InconsistentStateException) when (i < retries - 1)
                {
                    await LoadAsync(ct);
                }
            }
        }

        public async Task<TResult> UpdateAsync<TResult>(Func<T, (bool, TResult)> updater, int retries = 20,
            CancellationToken ct = default)
        {
            await EnsureLoadedAsync(ct);

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

        private async Task EnsureLoadedAsync(
            CancellationToken ct)
        {
            if (isLoaded)
            {
                return;
            }

            await LoadAsync(ct);
        }
    }
}
