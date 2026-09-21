// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetResizeGate : IDisposable
{
    private readonly Dictionary<string, Entry> entries = [];
    private readonly Lock lockObject = new Lock();
    private readonly SemaphoreSlim concurrencyGate;

    private sealed class Entry
    {
        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

        public int Users { get; set; }
    }

    private sealed class Lease(AssetResizeGate gate, string key, Entry entry) : IDisposable
    {
        public void Dispose()
        {
            gate.concurrencyGate.Release();
            entry.Semaphore.Release();
            gate.Return(key, entry);
        }
    }

    public AssetResizeGate(IOptions<AssetOptions> options)
    {
        var maxConcurrency = options.Value.MaxConcurrentResizes;

        // Resizing is CPU and memory bound, therefore the number of cores is a reasonable default.
        if (maxConcurrency <= 0)
        {
            maxConcurrency = Math.Max(2, Environment.ProcessorCount);
        }

        concurrencyGate = new SemaphoreSlim(maxConcurrency);
    }

    public void Dispose()
    {
        concurrencyGate.Dispose();
    }

    public async Task<IDisposable> AcquireAsync(string key,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        var entry = Rent(key);

        // Only one request per key resizes, the others wait here and then find the stored file.
        try
        {
            await entry.Semaphore.WaitAsync(ct);
        }
        catch
        {
            Return(key, entry);
            throw;
        }

        // Waiting requests do not hold a slot of the global limit, because they do not resize.
        try
        {
            await concurrencyGate.WaitAsync(ct);
        }
        catch
        {
            entry.Semaphore.Release();
            Return(key, entry);
            throw;
        }

        return new Lease(this, key, entry);
    }

    private Entry Rent(string key)
    {
        lock (lockObject)
        {
            if (!entries.TryGetValue(key, out var entry))
            {
                entry = new Entry();
                entries[key] = entry;
            }

            entry.Users++;
            return entry;
        }
    }

    private void Return(string key, Entry entry)
    {
        lock (lockObject)
        {
            entry.Users--;

            // The dictionary must not grow with the number of requested asset variants.
            if (entry.Users <= 0)
            {
                entries.Remove(key);
                entry.Semaphore.Dispose();
            }
        }
    }
}
