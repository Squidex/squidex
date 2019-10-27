// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public static class StoreExtensions
    {
        public static Task WriteEventAsync<T>(this IPersistence<T> persistence, Envelope<IEvent> @event)
        {
            return persistence.WriteEventsAsync(new[] { @event });
        }

        public static Task ClearSnapshotsAsync<TKey, TSnapshot>(this IStore<TKey> store)
        {
            return store.GetSnapshotStore<TSnapshot>().ClearAsync();
        }

        public static Task RemoveSnapshotAsync<TKey, TSnapshot>(this IStore<TKey> store, TKey key)
        {
            return store.GetSnapshotStore<TSnapshot>().RemoveAsync(key);
        }

        public static async Task<TSnapshot> GetSnapshotAsync<TKey, TSnapshot>(this IStore<TKey> store, TKey key)
        {
            var result = await store.GetSnapshotStore<TSnapshot>().ReadAsync(key);

            return result.Value;
        }
    }
}
