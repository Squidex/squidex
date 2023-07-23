﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Shared;

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
        // Sometimes we just want to skip this for performance reasons.
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

            // Script vars are just wrappers over dictionaries for better performance.
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
        // Script vars are just wrappers over dictionaries for better performance.
        var vars = new ContentScriptVars
        {
            ContentId = content.Id,
            Data = content.Data,
            DataOld = default,
            Status = content.Status,
            StatusOld = default
        };

        vars.CopyFrom(sharedVars);

        var options = new ScriptOptions
        {
            AsContext = true,
            CanDisallow = true,
            CanReject = true
        };

        content.Data = await scriptEngine.TransformAsync(vars, script, options, ct);
    }

    private static bool ShouldEnrich(Context context)
    {
        // We need a special permission to disable scripting for security reasons, if the script removes sensible data.
        var shouldScript =
            !context.NoScripting() ||
            !context.UserPermissions.Allows(PermissionIds.ForApp(PermissionIds.AppNoScripting, context.App.Name));

        return !context.IsFrontendClient && shouldScript;
    }
}
