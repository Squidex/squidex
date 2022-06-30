// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public sealed class SimpleState<T> where T : class, new()
    {
        private readonly IPersistence<T> persistence;

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

        public Task LoadAsync(
            CancellationToken ct = default)
        {
            return persistence.ReadAsync(ct: ct);
        }

        public Task ClearAsync()
        {
            Value = new T();

            return persistence.DeleteAsync();
        }

        public Task WriteAsync()
        {
            return persistence.WriteSnapshotAsync(Value);
        }

        public async Task UpdateAsync(Action<T> updater, int retries = 5)
        {
            for (var i = 0; i < retries; i++)
            {
                try
                {
                    updater(Value);
                }
                catch (InconsistentStateException) when (i < retries)
                {
                    await LoadAsync();
                }
            }
        }
    }
}
