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
    public sealed class AssetFolderResolver : IAssetFolderResolver
    {
        private static readonly char[] TrimChars = { '/', '\\' };
        private static readonly char[] SplitChars = { ' ', '/', '\\' };
        private readonly ILocalCache localCache;
        private readonly IAssetQueryService assetQuery;

        public AssetFolderResolver(ILocalCache localCache, IAssetQueryService assetQuery)
        {
            Guard.NotNull(localCache, nameof(localCache));
            Guard.NotNull(assetQuery, nameof(assetQuery));

            this.localCache = localCache;
            this.assetQuery = assetQuery;
        }

        public async Task<DomainId> ResolveOrCreateAsync(Context context, ICommandBus commandBus, string path)
        {
            Guard.NotNull(commandBus, nameof(commandBus));
            Guard.NotNull(path, nameof(path));

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

            var creating = false;

            for (; i < elements.Length; i++)
            {
                var name = elements[i];

                var isResolved = false;

                if (!creating)
                {
                    var children = await assetQuery.QueryAssetFoldersAsync(context, currentId);

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
                    creating = true;
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
