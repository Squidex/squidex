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

public sealed class UpdateValues : IContentValueConverter
{
    private readonly ContentData existingData;
    private readonly IScriptEngine scriptEngine;
    private ScriptVars? vars;

    public UpdateValues(ContentData existingData, IScriptEngine scriptEngine)
    {
        this.existingData = existingData;
        this.scriptEngine = scriptEngine;
    }

    public (bool Remove, JsonValue) ConvertValue(IField field, JsonValue source, IField? parent)
    {
        if (source.Value is not JsonObject jsonObject)
        {
            return (false, source);
        }

        if (jsonObject.TryGetValue("$unset", out var value1) && !Equals(value1.Value, false))
        {
            return (true, source);
        }

        if (!jsonObject.TryGetValue("$update", out var value2) || value2.Value is not string update)
        {
            return (false, source);
        }

        var options = new ScriptOptions { Readonly = true };

        vars ??= new ScriptVars
        {
            ["$data"] = existingData,
            ["$self"] = jsonObject
        };

        var result = scriptEngine.Execute(vars, update, options);

        return (false, result);
    }
}
