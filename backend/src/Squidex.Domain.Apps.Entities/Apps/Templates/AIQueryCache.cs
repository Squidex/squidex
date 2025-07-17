// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using IdentityModel;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Squidex.CLI.Commands.Implementation.AI;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

public sealed class AIQueryCache(IDistributedCache distributedCache) : IQueryCache
{
    private readonly DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30),
    };

    public async Task<GeneratedContent?> GetAsync(string prompt,
        CancellationToken ct = default)
    {
        var cached = await distributedCache.GetAsync(CacheKey(prompt), ct);
        if (cached == null)
        {
            return default!;
        }

        try
        {
            // Use newtonsoft JSON because the CLI still deals with this library and uses JTokens.
            using var cacheStream = new MemoryStream(cached);
            using var cacheReader = new StreamReader(cacheStream);
            using var jsonReader = new JsonTextReader(cacheReader);

            var serializer = new JsonSerializer();
            return serializer.Deserialize<GeneratedContent>(jsonReader);
        }
        catch
        {
            return default!;
        }
    }

    public async Task StoreAsync(string prompt, GeneratedContent content,
        CancellationToken ct)
    {
        try
        {
            // Use newtonsoft JSON because the CLI still deals with this library and uses JTokens.
            using var cacheStream = DefaultPools.MemoryStream.GetStream();
#pragma warning disable MA0042 // Do not use blocking calls in an async method
            using var cacheWriter = new StreamWriter(cacheStream, Encoding.UTF8, leaveOpen: true);

            using (var jsonWriter = new JsonTextWriter(cacheWriter))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, content);
                jsonWriter.Flush();
            }
#pragma warning restore MA0042 // Do not use blocking calls in an async method

            await distributedCache.SetAsync(CacheKey(prompt), cacheStream.ToArray(), cacheOptions, ct);
        }
        catch
        {
            return;
        }
    }

    private static string CacheKey(string prompt)
    {
        return $"AI_{prompt.ToSha512()}";
    }
}
