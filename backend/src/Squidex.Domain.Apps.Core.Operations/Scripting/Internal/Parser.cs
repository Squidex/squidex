// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Esprima;
using Esprima.Ast;
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

    public Script Parse(string script)
    {
        var cacheKey = $"{typeof(Parser)}_Script_{script}";

        return cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            return new JavaScriptParser().ParseScript(script);
        })!;
    }
}
