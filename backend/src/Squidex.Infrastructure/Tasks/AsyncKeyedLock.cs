// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Tasks;

public sealed class AsyncKeyedLock<TKey> where TKey : notnull
{
    private readonly Dictionary<TKey, Entry> entries = [];

    private sealed class Entry
    {
        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

        public int References { get; set; }
    }

    private sealed class Releaser(AsyncKeyedLock<TKey> owner, TKey key, Entry entry) : IDisposable
    {
        private int isDisposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref isDisposed, 1) == 0)
            {
                owner.Release(key, entry, true);
            }
        }
    }

    public int Count
    {
        get
        {
            lock (entries)
            {
                return entries.Count;
            }
        }
    }

    public async Task<IDisposable> EnterAsync(TKey key,
        CancellationToken ct = default)
    {
        Entry? entry;

        lock (entries)
        {
            if (!entries.TryGetValue(key, out entry))
            {
                entry = new Entry();
                entries[key] = entry;
            }

            entry.References++;
        }

        try
        {
            await entry.Semaphore.WaitAsync(ct);
        }
        catch
        {
            Release(key, entry, false);
            throw;
        }

        return new Releaser(this, key, entry);
    }

    private void Release(TKey key, Entry entry, bool acquired)
    {
        lock (entries)
        {
            if (acquired)
            {
                entry.Semaphore.Release();
            }

            // Remove unused entries, because the number of keys is not limited.
            entry.References--;

            if (entry.References == 0)
            {
                entries.Remove(key);
                entry.Semaphore.Dispose();
            }
        }
    }
}
