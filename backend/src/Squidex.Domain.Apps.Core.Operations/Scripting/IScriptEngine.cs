// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting;

public interface IScriptEngine
{
    IScript CreateScript(string script, ScriptOptions options = default);

    IAsyncScript CreateAsyncScript(string script, ScriptOptions options = default);

    ContentData Transform(DataScriptVars vars, string script, ScriptOptions options = default)
    {
        using var compiled = CreateScript(script, options);

        return compiled.Transform(vars);
    }

    JsonValue Execute(ScriptVars vars, string script, ScriptOptions options = default)
    {
        using var compiled = CreateScript(script, options);

        return compiled.Execute(vars);
    }

    bool Evaluate(ScriptVars vars, string script, ScriptOptions options = default)
    {
        try
        {
            using var compiled = CreateScript(script, options);

            return compiled.Evaluate(vars);
        }
        catch
        {
            return false;
        }
    }

    async ValueTask<ContentData> TransformAsync(DataScriptVars vars, string script, ScriptOptions options = default,
        CancellationToken ct = default)
    {
        using var compiled = CreateAsyncScript(script, options);

        return await compiled.TransformAsync(vars, ct);
    }

    async ValueTask<JsonValue> ExecuteAsync(ScriptVars vars, string script, ScriptOptions options = default,
        CancellationToken ct = default)
    {
        using var compiled = CreateAsyncScript(script, options);

        return await compiled.ExecuteAsync(vars, ct);
    }

    async ValueTask<bool> EvaluateAsync(ScriptVars vars, string script, ScriptOptions options = default,
        CancellationToken ct = default)
    {
        try
        {
            using var compiled = CreateAsyncScript(script, options);

            return await compiled.EvaluateAsync(vars, ct);
        }
        catch
        {
            return false;
        }
    }
}
