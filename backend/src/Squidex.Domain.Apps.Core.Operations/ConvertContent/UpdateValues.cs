// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class UpdateValues : IContentValueConverter, IContentDataConverter
{
    private readonly ContentData existingData;
    private readonly IScriptEngine scriptEngine;
    private readonly bool canUnset;
    private ScriptVars? vars;

    public UpdateValues(ContentData existingData, IScriptEngine scriptEngine, bool canUnset)
    {
        this.existingData = existingData;
        this.scriptEngine = scriptEngine;
        this.canUnset = canUnset;
    }

    public void ConvertDataBefore(Schema schema, ContentData source)
    {
        // Avoid unnecessary allocations if nothing has been changed, which is the default.
        List<string>? toRemove = null;
        List<(string, ContentFieldData)>? toReplace = null;

        foreach (var (key, value) in source)
        {
            if (canUnset && Updates.IsUnset(value))
            {
                toRemove ??= [];
                toRemove.Add(key);
            }

            if (Updates.IsUpdate(value, out var expression))
            {
                var options = new ScriptOptions { Readonly = true };

                // Reuse the vars to save allocations.
                vars ??= new ScriptVars
                {
                    ["$data"] = existingData
                };

                // Give access to the current update statement to carry extra values from the request.
                vars["$self"] = value;

                // Put the expression in brackets to return an object directly.
                var result = scriptEngine.Execute(vars, $"({expression})", options);

                if (result.Value is JsonObject obj)
                {
                    var replacement = new ContentFieldData(obj);

                    if (!replacement.Equals(value))
                    {
                        toReplace ??= [];
                        toReplace.Add((key, replacement));
                    }
                }
            }
        }

        if (toRemove != null)
        {
            foreach (var key in toRemove)
            {
                source.Remove(key);
            }
        }

        if (toReplace != null)
        {
            foreach (var (key, value) in toReplace)
            {
                source[key] = value;
            }
        }
    }

    public (bool Remove, JsonValue) ConvertValue(IField field, JsonValue source, IField? parent)
    {
        if (source.Value is not JsonObject obj)
        {
            return (false, source);
        }

        if (canUnset && Updates.IsUnset(obj))
        {
            return (true, source);
        }

        if (!Updates.IsUpdate(obj, out var expression))
        {
            return (false, source);
        }

        var options = new ScriptOptions { Readonly = true };

        // Reuse the vars to save allocations.
        vars ??= new ScriptVars
        {
            ["$data"] = existingData,
        };

        // Give access to the current update statement to carry extra values from the request.
        vars["$self"] = obj;

        // Put the expression in brackets to return an object directly.
        var result = scriptEngine.Execute(vars, $"({expression})", options);

        return (false, result);
    }
}
