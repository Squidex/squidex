// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Esprima.Ast;
using Jint;
using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

internal sealed class Parser
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IMemoryCache cache;

    public Parser(IMemoryCache cache)
    {
        this.cache = cache;
    }

    public Prepared<Script> Parse(string script)
    {
        var cacheKey = $"{typeof(Parser)}_Script_{script}";

        return cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            return Engine.PrepareScript(script);
        })!;
    }
}
