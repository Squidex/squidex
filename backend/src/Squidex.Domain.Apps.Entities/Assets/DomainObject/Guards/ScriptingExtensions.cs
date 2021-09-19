// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
{
    public static class ScriptingExtensions
    {
        private static readonly ScriptOptions Options = new ScriptOptions
        {
            AsContext = true,
            CanDisallow = true,
            CanReject = true
        };

        private static class ScriptKeys
        {
            public const string AppId = "appId";
            public const string AppName = "appName";
            public const string Asset = "asset";
            public const string AssetId = "assetId";
            public const string Command = "command";
            public const string FileHash = "fileHash";
            public const string FileName = "fileName";
            public const string FileSize = "fileSize";
            public const string FileSlug = "fileSlug";
            public const string FileVersion = "fileVersion";
            public const string IsProtected = "isProtected";
            public const string Metadata = "metadata";
            public const string MimeType = "mimeType";
            public const string Operation = "operation";
            public const string ParentId = "parentId";
            public const string ParentPath = "parentPath";
            public const string Permanent = "permanent";
            public const string Tags = "tags";
            public const string User = "User";
        }

        public static async Task ExecuteCreateScriptAsync(this AssetOperation operation, CreateAsset create)
        {
            var script = operation.App.AssetScripts?.Create;

            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            var parentPath = await GetPathAsync(operation, create.ParentId);

            // Tags and metadata are mutable and can be changed from the scripts, but not replaced.
            var vars = new ScriptVars
            {
                // Use a dictionary for better performance, because no reflection is involved.
                [ScriptKeys.Command] = new Dictionary<string, object?>
                {
                    [ScriptKeys.Metadata] = create.Metadata.Mutable(),
                    [ScriptKeys.FileHash] = create.FileHash,
                    [ScriptKeys.FileName] = create.File.FileName,
                    [ScriptKeys.FileSize] = create.File.FileSize,
                    [ScriptKeys.FileSlug] = create.File.FileName.Slugify(),
                    [ScriptKeys.MimeType] = create.File.MimeType,
                    [ScriptKeys.ParentId] = create.ParentId,
                    [ScriptKeys.ParentPath] = parentPath,
                    [ScriptKeys.Tags] = create.Tags
                },
                [ScriptKeys.Operation] = "Create"
            };

            await ExecuteScriptAsync(operation, script, vars);
        }

        public static Task ExecuteUpdateScriptAsync(this AssetOperation operation, UpdateAsset update)
        {
            var script = operation.App.AssetScripts?.Update;

            if (string.IsNullOrWhiteSpace(script))
            {
                return Task.CompletedTask;
            }

            // Tags and metadata are mutable and can be changed from the scripts, but not replaced.
            var vars = new ScriptVars
            {
                // Use a dictionary for better performance, because no reflection is involved.
                [ScriptKeys.Command] = new Dictionary<string, object?>
                {
                    [ScriptKeys.Metadata] = update.Metadata.Mutable(),
                    [ScriptKeys.FileHash] = update.FileHash,
                    [ScriptKeys.FileName] = update.File.FileName,
                    [ScriptKeys.FileSize] = update.File.FileSize,
                    [ScriptKeys.MimeType] = update.File.MimeType,
                    [ScriptKeys.Tags] = update.Tags
                },
                [ScriptKeys.Operation] = "Update"
            };

            return ExecuteScriptAsync(operation, script, vars);
        }

        public static Task ExecuteAnnotateScriptAsync(this AssetOperation operation, AnnotateAsset annotate)
        {
            var script = operation.App.AssetScripts?.Annotate;

            if (string.IsNullOrWhiteSpace(script))
            {
                return Task.CompletedTask;
            }

            // Tags are mutable and can be changed from the scripts, but not replaced.
            var vars = new ScriptVars
            {
                // Use a dictionary for better performance, because no reflection is involved.
                [ScriptKeys.Command] = new Dictionary<string, object?>
                {
                    [ScriptKeys.Metadata] = annotate.Metadata?.Mutable(),
                    [ScriptKeys.FileName] = annotate.FileName,
                    [ScriptKeys.FileSlug] = annotate.Slug,
                    [ScriptKeys.Tags] = annotate.Tags
                },
                [ScriptKeys.Operation] = "Annotate"
            };

            return ExecuteScriptAsync(operation, script, vars);
        }

        public static async Task ExecuteMoveScriptAsync(this AssetOperation operation, MoveAsset move)
        {
            var script = operation.App.AssetScripts?.Move;

            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            var parentPath = await GetPathAsync(operation, move.ParentId);

            var vars = new ScriptVars
            {
                // Use a dictionary for better performance, because no reflection is involved.
                [ScriptKeys.Command] = new Dictionary<string, object?>
                {
                    [ScriptKeys.ParentId] = move.ParentId,
                    [ScriptKeys.ParentPath] = parentPath
                },
                [ScriptKeys.Operation] = "Move"
            };

            await ExecuteScriptAsync(operation, script, vars);
        }

        public static Task ExecuteDeleteScriptAsync(this AssetOperation operation, DeleteAsset delete)
        {
            var script = operation.App.AssetScripts?.Delete;

            if (string.IsNullOrWhiteSpace(script))
            {
                return Task.CompletedTask;
            }

            var vars = new ScriptVars
            {
                // Use a dictionary for better performance, because no reflection is involved.
                [ScriptKeys.Command] = new Dictionary<string, object?>
                {
                    [ScriptKeys.Permanent] = delete.Permanent
                },
                [ScriptKeys.Operation] = "Delete"
            };

            return ExecuteScriptAsync(operation, script, vars);
        }

        private static async Task ExecuteScriptAsync(AssetOperation operation, string script, ScriptVars vars)
        {
            var snapshot = operation.Snapshot;

            var parentPath = await GetPathAsync(operation, snapshot.ParentId);

            // Use a dictionary for better performance, because no reflection is involved.
            var asset = new Dictionary<string, object?>
            {
                [ScriptKeys.Metadata] = snapshot.ReadonlyMetadata(),
                [ScriptKeys.FileHash] = snapshot.FileHash,
                [ScriptKeys.FileName] = snapshot.FileName,
                [ScriptKeys.FileSize] = snapshot.FileSize,
                [ScriptKeys.FileSlug] = snapshot.Slug,
                [ScriptKeys.FileVersion] = snapshot.FileVersion,
                [ScriptKeys.IsProtected] = snapshot.IsProtected,
                [ScriptKeys.MimeType] = snapshot.MimeType,
                [ScriptKeys.ParentId] = snapshot.ParentId,
                [ScriptKeys.ParentPath] = parentPath,
                [ScriptKeys.Tags] = snapshot.ReadonlyTags()
            };

            vars[ScriptKeys.AppId] = operation.App.Id;
            vars[ScriptKeys.AppName] = operation.App.Name;
            vars[ScriptKeys.AssetId] = operation.CommandId;
            vars[ScriptKeys.Asset] = asset;
            vars[ScriptKeys.User] = operation.User;

            var scriptEngine = operation.Resolve<IScriptEngine>();

            await scriptEngine.ExecuteAsync(vars, script, Options);
        }

        private static async Task<object> GetPathAsync(AssetOperation operation, DomainId parentId)
        {
            if (parentId == default)
            {
                return Array.Empty<object>();
            }

            var assetQuery = operation.Resolve<IAssetQueryService>();

            var path = await assetQuery.FindAssetFolderAsync(operation.App.Id, parentId);

            return path.Select(x => new { id = x.Id, folderName = x.FolderName }).ToList();
        }

        private static object? Mutable(this AssetMetadata metadata)
        {
            if (metadata == null)
            {
                return null;
            }

            return new ScriptMetadataWrapper(metadata);
        }

        private static object? ReadonlyMetadata(this IAssetEntity asset)
        {
            if (asset.Metadata == null)
            {
                return null;
            }

            return new ReadOnlyDictionary<string, IJsonValue>(asset.Metadata);
        }

        private static object? ReadonlyTags(this IAssetEntity asset)
        {
            if (asset.Tags == null)
            {
                return null;
            }

            return new ReadOnlyCollection<string>(asset.Tags.ToList());
        }
    }
}
