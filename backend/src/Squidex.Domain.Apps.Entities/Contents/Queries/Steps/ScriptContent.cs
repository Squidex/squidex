// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class ScriptContent : IContentEnricherStep
{
    private readonly IScriptEngine scriptEngine;

    public ScriptContent(IScriptEngine scriptEngine)
    {
        this.scriptEngine = scriptEngine;
    }

    public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        if (!ShouldEnrich(context))
        {
            return;
        }

        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            var (schema, _) = await schemas(group.Key);

            var script = schema.SchemaDef.Scripts.Query;

            if (string.IsNullOrWhiteSpace(script))
            {
                continue;
            }

            var vars = new ContentScriptVars
            {
                AppId = schema.AppId.Id,
                AppName = schema.AppId.Name,
                SchemaId = schema.Id,
                SchemaName = schema.SchemaDef.Name,
                User = context.UserPrincipal
            };

            var preScript = schema.SchemaDef.Scripts.QueryPre;

            if (!string.IsNullOrWhiteSpace(preScript))
            {
                var options = new ScriptOptions
                {
                    AsContext = true
                };

                await scriptEngine.ExecuteAsync(vars, preScript, options, ct);
            }

            foreach (var content in group)
            {
                await TransformAsync(vars, script, content, ct);
            }
        }
    }

    private async Task TransformAsync(ContentScriptVars sharedVars, string script, ContentEntity content,
        CancellationToken ct)
    {
        var vars = new ContentScriptVars
        {
            ContentId = content.Id,
            Data = content.Data,
            DataOld = default,
            Status = content.Status,
            StatusOld = default
        };

        foreach (var (key, value) in sharedVars)
        {
            if (!vars.ContainsKey(key))
            {
                vars[key] = value;
            }
        }

        var options = new ScriptOptions
        {
            AsContext = true
        };

        content.Data = await scriptEngine.TransformAsync(vars, script, options, ct);
    }

    private static bool ShouldEnrich(Context context)
    {
        return !context.IsFrontendClient;
    }
}
