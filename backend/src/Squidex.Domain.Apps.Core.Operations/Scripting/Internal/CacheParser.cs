// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Acornima.Ast;
using Jint;
using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

internal sealed class CacheParser(IMemoryCache cache)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public Prepared<Script> Parse(string script)
    {
        // A tuple keeps a reference to the source instead of copying it into a bigger string on
        // every execution, which also stops the cache from holding a second copy of every script.
        var cacheKey = (typeof(CacheParser), script);

        return cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            return Engine.PrepareScript(script);
        })!;
    }
}
