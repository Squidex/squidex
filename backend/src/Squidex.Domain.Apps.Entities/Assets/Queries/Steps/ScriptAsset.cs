// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure.Collections;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Assets.Queries.Steps;

public sealed class ScriptAsset : IAssetEnricherStep
{
    private readonly IScriptEngine scriptEngine;

    public ScriptAsset(IScriptEngine scriptEngine)
    {
        this.scriptEngine = scriptEngine;
    }

    public async Task EnrichAsync(Context context, IEnumerable<EnrichedAsset> assets,
        CancellationToken ct)
    {
        if (!ShouldEnrich(context))
        {
            return;
        }

        var script = context.App.AssetScripts.Query;

        if (string.IsNullOrWhiteSpace(script))
        {
            return;
        }

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = new AssetScriptVars
        {
            AppId = context.App.Id,
            AppName = context.App.Name,
            User = context.UserPrincipal
        };

        var preScript = context.App.AssetScripts.QueryPre;

        if (!string.IsNullOrWhiteSpace(preScript))
        {
            var options = new ScriptOptions
            {
                AsContext = true
            };

            await scriptEngine.ExecuteAsync(vars, preScript, options, ct);
        }

        foreach (var asset in assets)
        {
            await ScriptAsync(vars, script, asset, ct);
        }
    }

    private async Task ScriptAsync(AssetScriptVars sharedVars, string script, EnrichedAsset asset,
        CancellationToken ct)
    {
        // Script vars are just wrappers over dictionaries for better performance.
        var vars = new AssetScriptVars
        {
            AssetId = asset.Id,
            Asset = new AssetEntityScriptVars
            {
                Type = asset.Type,
                FileHash = asset.FileHash,
                FileName = asset.FileName,
                FileSize = asset.FileSize,
                FileSlug = asset.Slug,
                FileVersion = asset.FileVersion,
                IsProtected = asset.IsProtected,
                Metadata = asset.Metadata?.ToReadonlyDictionary(),
                MimeType = asset.MimeType,
                ParentId = asset.ParentId,
                ParentPath = null,
                Tags = asset.Tags?.ToReadonlyList()
            }
        };

        vars.CopyFrom(sharedVars);

        var options = new ScriptOptions
        {
            AsContext = true,
            CanDisallow = true,
            CanReject = true
        };

        await scriptEngine.ExecuteAsync(vars, script, options, ct);
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
