// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed class AssetFolderResolverCommandMiddleware : ICommandMiddleware
    {
        private static readonly char[] TrimChars = { '/', '\\' };
        private static readonly char[] SplitChars = { ' ', '/', '\\' };
        private readonly ILocalCache localCache;
        private readonly IAssetQueryService assetQuery;

        public AssetFolderResolverCommandMiddleware(ILocalCache localCache, IAssetQueryService assetQuery)
        {
            Guard.NotNull(localCache, nameof(localCache));
            Guard.NotNull(assetQuery, nameof(assetQuery));

            this.localCache = localCache;
            this.assetQuery = assetQuery;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            switch (context.Command)
            {
                case IMoveAssetCommand move:
                    if (!string.IsNullOrWhiteSpace(move.ParentPath))
                    {
                        move.ParentId = await ResolveOrCreateAsync(context.CommandBus, move.AppId.Id, move.ParentPath);
                    }

                    break;

                case UpsertAsset upsert:
                    if (!string.IsNullOrWhiteSpace(upsert.ParentPath))
                    {
                        upsert.ParentId = await ResolveOrCreateAsync(context.CommandBus, upsert.AppId.Id, upsert.ParentPath);
                    }

                    break;
            }
        }

        private async Task<DomainId> ResolveOrCreateAsync(ICommandBus commandBus, DomainId appId, string path)
        {
            path = path.Trim(TrimChars);

            var elements = path.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);

            if (elements.Length == 0)
            {
                return DomainId.Empty;
            }

            var currentId = DomainId.Empty;

            var i = elements.Length;

            for (; i > 0; i--)
            {
                var subPath = string.Join('/', elements.Take(i));

                if (localCache.TryGetValue(GetCacheKey(subPath), out var cached) && cached is DomainId id)
                {
                    currentId = id;
                    break;
                }
            }

            var currentCreated = false;

            for (; i < elements.Length; i++)
            {
                var name = elements[i];

                var isResolved = false;

                if (!currentCreated)
                {
                    var children = await assetQuery.QueryAssetFoldersAsync(appId, currentId);

                    foreach (var child in children)
                    {
                        var childPath = string.Join('/', elements.Take(i).Union(Enumerable.Repeat(child.FolderName, 1)));

                        localCache.Add(GetCacheKey(childPath), child.Id);
                    }

                    foreach (var child in children)
                    {
                        if (child.FolderName == name)
                        {
                            currentId = child.Id;

                            isResolved = true;
                            break;
                        }
                    }
                }

                if (!isResolved)
                {
                    var command = new CreateAssetFolder { ParentId = currentId, FolderName = name };

                    await commandBus.PublishAsync(command);

                    currentId = command.AssetFolderId;
                    currentCreated = true;
                }

                var newPath = string.Join('/', elements.Take(i).Union(Enumerable.Repeat(name, 1)));

                localCache.Add(GetCacheKey(newPath), currentId);
            }

            return currentId;
        }

        private static object GetCacheKey(string path)
        {
            return $"ASSET_FOLDERS_{path}";
        }
    }
}
