// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Esprima;
using Esprima.Ast;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.Internal
{
    internal sealed class Parser
    {
        private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(10);
        private static readonly ParserOptions DefaultParserOptions = new ParserOptions
        {
            AdaptRegexp = true, Tolerant = true, Loc = true
        };

        private readonly IMemoryCache memoryCache;

        public Parser(IMemoryCache memoryCache)
        {
            Guard.NotNull(memoryCache, nameof(memoryCache));

            this.memoryCache = memoryCache;
        }

        public Script Parse(string script)
        {
            var key = Key(script);

            if (!memoryCache.TryGetValue<Script>(key, out var compiledScript))
            {
                var parser = new JavaScriptParser(script, DefaultParserOptions);

                compiledScript = parser.ParseScript();

                memoryCache.Set(key, compiledScript, Expiration);
            }

            return compiledScript;
        }

        private static string Key(string script)
        {
            return $"SCRIPT_{script}";
        }
    }
}
