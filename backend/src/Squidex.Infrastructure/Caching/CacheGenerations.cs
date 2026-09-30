// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Hosting;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.Caching;

public sealed class CacheGenerations(
    IDistributedCache cache,
    IOptions<CacheGenerationsOptions> options,
    ILogger<CacheGenerations> log)
    : ICacheGenerations, IBackgroundProcess
{
    // Without an expiration the generation lives forever. A lost generation is replaced by a new
    // random value, so that entries of an old generation can never be read again.
    private static readonly DistributedCacheEntryOptions EntryOptions = new DistributedCacheEntryOptions();
    private readonly ConcurrentDictionary<string, bool> pendingKeys = new ConcurrentDictionary<string, bool>();
    private CompletionTimer? timer;

    public Task StartAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer(options.Value.ResetInterval, FlushAsync);

        return Task.CompletedTask;
    }

    public async Task StopAsync(
        CancellationToken ct)
    {
        if (timer != null)
        {
            await timer.StopAsync();
        }

        // Do not lose the resets of the last interval.
        await FlushAsync(ct);
    }

    public async Task<string> GetAsync(string key,
        CancellationToken ct = default)
    {
        var generation = await cache.GetStringAsync(CacheKey(key), ct);
        if (generation != null)
        {
            return generation;
        }

        return await WriteAsync(key, ct);
    }

    public void Reset(string key)
    {
        // Many resets in a short time, for example from bulk updates, are written once per interval.
        pendingKeys[key] = true;
    }

    public async Task FlushAsync(
        CancellationToken ct)
    {
        foreach (var key in pendingKeys.Keys)
        {
            if (!pendingKeys.TryRemove(key, out _))
            {
                continue;
            }

            try
            {
                await WriteAsync(key, ct);
            }
            catch (Exception ex)
            {
                // The callers have changed data already and must not fail because of the cache.
                log.LogWarning(ex, "Failed to reset cache generation {key}.", key);
            }
        }
    }

    private async Task<string> WriteAsync(string key,
        CancellationToken ct)
    {
        var generation = Guid.NewGuid().ToString();

        await cache.SetStringAsync(CacheKey(key), generation, EntryOptions, ct);

        return generation;
    }

    private static string CacheKey(string key)
    {
        return $"generation/{key}";
    }
}
