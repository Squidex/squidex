// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;

public static class ScriptingExtensions
{
    private static readonly ScriptOptions Options = new ScriptOptions
    {
        AsContext = true,
        CanDisallow = true,
        CanReject = true
    };

    public static async Task ExecuteCreateScriptAsync(this AssetOperation operation, CreateAsset create,
        CancellationToken ct)
    {
        var script = operation.App.AssetScripts?.Create;

        if (string.IsNullOrWhiteSpace(script))
        {
            return;
        }

        var parentPath = await GetPathAsync(operation, create.ParentId, ct);

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = new AssetScriptVars
        {
            FileId = create.FileId,
            // Tags and metadata are mutable and can be changed from the scripts, but not replaced.
            Command = new AssetCommandScriptVars
            {
                FileHash = create.FileHash,
                FileName = create.File.FileName,
                FileSlug = create.File.FileName.Slugify(),
                FileSize = create.File.FileSize,
                Metadata = create.Metadata,
                MimeType = create.File.MimeType,
                ParentId = create.ParentId,
                ParentPath = parentPath,
                Tags = create.Tags
            },
            Operation = "Create"
        };

        var asset = new AssetEntityScriptVars
        {
            Type = create.Type,
            FileHash = create.FileHash,
            FileName = create.File.FileName,
            FileSlug = create.File.FileName.Slugify(),
            FileSize = create.File.FileSize,
            FileVersion = 0,
            IsProtected = false,
            Metadata = create.Metadata?.ToReadonlyDictionary(),
            MimeType = create.File.MimeType,
            ParentId = create.ParentId,
            ParentPath = await GetPathAsync(operation, create.ParentId, ct),
            Tags = create.Tags?.ToReadonlyList()
        };

        await ExecuteScriptAsync(operation, script, vars, asset, ct);
    }

    public static Task ExecuteUpdateScriptAsync(this AssetOperation operation, UpdateAsset update,
        CancellationToken ct)
    {
        var script = operation.App.AssetScripts?.Update;

        if (string.IsNullOrWhiteSpace(script))
        {
            return Task.CompletedTask;
        }

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = new AssetScriptVars
        {
            FileId = update.FileId,
            // Tags and metadata are mutable and can be changed from the scripts, but not replaced.
            Command = new AssetCommandScriptVars
            {
                Metadata = update.Metadata,
                FileHash = update.FileHash,
                FileName = update.File.FileName,
                FileSize = update.File.FileSize,
                MimeType = update.File.MimeType,
                Tags = update.Tags
            },
            Operation = "Update"
        };

        return ExecuteScriptAsync(operation, script, vars, null, ct);
    }

    public static Task ExecuteAnnotateScriptAsync(this AssetOperation operation, AnnotateAsset annotate,
        CancellationToken ct)
    {
        var script = operation.App.AssetScripts?.Annotate;

        if (string.IsNullOrWhiteSpace(script))
        {
            return Task.CompletedTask;
        }

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = new AssetScriptVars
        {
            // Tags are mutable and can be changed from the scripts, but not replaced.
            Command = new AssetCommandScriptVars
            {
                IsProtected = annotate.IsProtected,
                Metadata = annotate.Metadata,
                FileName = annotate.FileName,
                FileSlug = annotate.Slug,
                Tags = annotate.Tags
            },
            Operation = "Annotate"
        };

        return ExecuteScriptAsync(operation, script, vars, null, ct);
    }

    public static async Task ExecuteMoveScriptAsync(this AssetOperation operation, MoveAsset move,
        CancellationToken ct)
    {
        var script = operation.App.AssetScripts?.Move;

        if (string.IsNullOrWhiteSpace(script))
        {
            return;
        }

        var parentPath = await GetPathAsync(operation, move.ParentId, ct);

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = new AssetScriptVars
        {
            Command = new AssetCommandScriptVars
            {
                ParentId = move.ParentId,
                ParentPath = parentPath
            },
            Operation = "Move"
        };

        await ExecuteScriptAsync(operation, script, vars, null, ct);
    }

    public static Task ExecuteDeleteScriptAsync(this AssetOperation operation, DeleteAsset delete,
        CancellationToken ct)
    {
        var script = operation.App.AssetScripts?.Delete;

        if (string.IsNullOrWhiteSpace(script))
        {
            return Task.CompletedTask;
        }

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = new AssetScriptVars
        {
            Command = new AssetCommandScriptVars
            {
                Permanent = delete.Permanent
            },
            Operation = "Delete"
        };

        return ExecuteScriptAsync(operation, script, vars, null, ct);
    }

    private static async Task ExecuteScriptAsync(AssetOperation operation, string script, AssetScriptVars vars, AssetEntityScriptVars? asset,
        CancellationToken ct)
    {
        var snapshot = operation.Snapshot;

        // Script vars are just wrappers over dictionaries for better performance.
        asset ??= new AssetEntityScriptVars
        {
            Type = snapshot.Type,
            FileHash = snapshot.FileHash,
            FileName = snapshot.FileName,
            FileSize = snapshot.FileSize,
            FileSlug = snapshot.Slug,
            FileVersion = snapshot.FileVersion,
            IsProtected = snapshot.IsProtected,
            Metadata = snapshot.Metadata?.ToReadonlyDictionary(),
            MimeType = snapshot.MimeType,
            ParentId = snapshot.ParentId,
            ParentPath = await GetPathAsync(operation, snapshot.ParentId, ct),
            Tags = snapshot.Tags?.ToReadonlyList(),
        };

        vars.AppId = operation.App.Id;
        vars.AppName = operation.App.Name;
        vars.AssetId = operation.CommandId;
        vars.Asset = asset;
        vars.User = operation.User;

        var scriptEngine = operation.Resolve<IScriptEngine>();

        await scriptEngine.ExecuteAsync(vars, script, Options, ct);
    }

    private static async Task<Array> GetPathAsync(AssetOperation operation, DomainId parentId,
        CancellationToken ct)
    {
        if (parentId == default)
        {
            return Array.Empty<object>();
        }

        var assetQuery = operation.Resolve<IAssetQueryService>();
        var assetPath = await assetQuery.FindAssetFolderAsync(operation.App.Id, parentId, ct);

        return assetPath.Select(x => new { id = x.Id, folderName = x.FolderName }).ToArray();
    }
}
