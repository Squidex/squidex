// ==========================================================================
//  QueryContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Infrastructure;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Assets.Repositories;

// ReSharper disable InvertIf

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public sealed class QueryContext
    {
        private readonly ConcurrentDictionary<Guid, IContentEntity> cachedContents = new ConcurrentDictionary<Guid, IContentEntity>();
        private readonly ConcurrentDictionary<Guid, IAssetEntity> cachedAssets = new ConcurrentDictionary<Guid, IAssetEntity>();
        private readonly IContentRepository contentRepository;
        private readonly IAssetRepository assetRepository;
        private readonly IAppEntity app;

        public QueryContext(IAppEntity app, IContentRepository contentRepository, IAssetRepository assetRepository)
        {
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(app, nameof(app));

            this.contentRepository = contentRepository;
            this.assetRepository = assetRepository;

            this.app = app;
        }

        public async Task<IAssetEntity> FindAssetAsync(Guid id)
        {
            var asset = cachedAssets.GetOrDefault(id);

            if (asset == null)
            {
                asset = await assetRepository.FindAssetAsync(id);

                if (asset != null)
                {
                    cachedAssets[asset.Id] = asset;
                }
            }

            return asset;
        }

        public async Task<IContentEntity> FindContentAsync(Guid schemaId, Guid id)
        {
            var content = cachedContents.GetOrDefault(id);

            if (content == null)
            {
                content = await contentRepository.FindContentAsync(app, schemaId, id);

                if (content != null)
                {
                    cachedContents[content.Id] = content;
                }
            }

            return content;
        }

        public async Task<IReadOnlyList<IAssetEntity>> QueryAssetsAsync(string query, int skip = 0, int take = 10)
        {
            var assets = await assetRepository.QueryAsync(app.Id, null, null, query, take, skip);

            foreach (var asset in assets)
            {
                cachedAssets[asset.Id] = asset;
            }

            return assets;
        }

        public async Task<IReadOnlyList<IContentEntity>> QueryContentsAsync(Guid schemaId, string query)
        {
            var contents = await contentRepository.QueryAsync(app, schemaId, false, null, query);

            foreach (var content in contents)
            {
                cachedContents[content.Id] = content;
            }

            return contents;
        }

        public List<IAssetEntity> GetReferencedAssets(JToken value)
        {
            var ids = ParseIds(value);

            return GetReferencedAssets(ids);
        }

        public List<IAssetEntity> GetReferencedAssets(ICollection<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedAssets = new HashSet<Guid>(ids.Where(id => !cachedAssets.ContainsKey(id)));

            if (notLoadedAssets.Count > 0)
            {
                Task.Run(async () =>
                {
                    var assets = await assetRepository.QueryAsync(app.Id, null, notLoadedAssets, null, int.MaxValue).ConfigureAwait(false);

                    foreach (var asset in assets)
                    {
                        cachedAssets[asset.Id] = asset;
                    }
                }).Wait();
            }

            return ids.Select(id => cachedAssets.GetOrDefault(id)).Where(x => x != null).ToList();
        }

        public List<IContentEntity> GetReferencedContents(Guid schemaId, JToken value)
        {
            var ids =  ParseIds(value);

            return GetReferencedContents(schemaId, ids);
        }

        public List<IContentEntity> GetReferencedContents(Guid schemaId, ICollection<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedContents = new HashSet<Guid>(ids.Where(id => !cachedContents.ContainsKey(id)));

            if (notLoadedContents.Count > 0)
            {
                Task.Run(async () =>
                {
                    var contents = await contentRepository.QueryAsync(app, schemaId, false, notLoadedContents, null).ConfigureAwait(false);

                    foreach (var content in contents)
                    {
                        cachedContents[content.Id] = content;
                    }
                }).Wait();
            }

            return ids.Select(id => cachedContents.GetOrDefault(id)).Where(x => x != null).ToList();
        }

        private static ICollection<Guid> ParseIds(JToken value)
        {
            try
            {
                var result = new List<Guid>();

                if (value is JArray)
                {
                    foreach (var id in value)
                    {
                        result.Add(Guid.Parse(id.ToString()));
                    }
                }

                return result;
            }
            catch
            {
                return new List<Guid>();
            }
        }
    }
}
