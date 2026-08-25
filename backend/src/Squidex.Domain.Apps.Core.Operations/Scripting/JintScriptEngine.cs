// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Properties;
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class JintScriptEngine(IMemoryCache cache, IOptions<JintScriptOptions> options, IEnumerable<IJintExtension>? extensions = null) : IScriptEngine, IScriptDescriptor
{
    private readonly IJintExtension[] extensions = extensions?.ToArray() ?? [];
    private readonly CacheParser parser = new CacheParser(cache);

    public IScript CreateScript(string script, ScriptOptions scriptOptions = default)
    {
        return CreateScriptCore(script, scriptOptions, false);
    }

    public IAsyncScript CreateAsyncScript(string script, ScriptOptions scriptOptions = default)
    {
        return CreateScriptCore(script, scriptOptions, true);
    }

    private JintScript CreateScriptCore(string script, ScriptOptions scriptOptions, bool allowAsync)
    {
        Guard.NotNullOrEmpty(script);

        try
        {
            // The parser caches the prepared script, therefore the same source is only parsed once.
            return new JintScript(parser.Parse(script), scriptOptions, options.Value, extensions, allowAsync);
        }
        catch (Exception ex)
        {
            throw JintScript.MapException(ex);
        }
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (scope.HasFlag(ScriptScope.ContentTrigger) || scope.HasFlag(ScriptScope.AssetTrigger))
        {
            return;
        }

        if (scope.HasFlag(ScriptScope.Transform) || scope.HasFlag(ScriptScope.ContentScript))
        {
            describe(JsonType.Function, "replace()",
                Resources.ScriptingReplace);
        }

        describe(JsonType.Function, "disallow(reason)",
            Resources.ScriptingDisallow);

        describe(JsonType.Function, "reject(reason)",
            Resources.ScriptingReject);

        describe(JsonType.Function, "complete()",
            Resources.ScriptingComplete);
    }
}
