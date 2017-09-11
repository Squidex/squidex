// ==========================================================================
//  QueryContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public sealed class QueryContext
    {
        private readonly ConcurrentDictionary<Guid, IContentEntity> cachedContents = new ConcurrentDictionary<Guid, IContentEntity>();
        private readonly ConcurrentDictionary<Guid, IAssetEntity> cachedAssets = new ConcurrentDictionary<Guid, IAssetEntity>();
        private readonly IContentQueryService contentQuery;
        private readonly IAssetRepository assetRepository;
        private readonly IGraphQLUrlGenerator urlGenerator;
        private readonly IAppEntity app;
        private readonly ClaimsPrincipal user;

        public IGraphQLUrlGenerator UrlGenerator
        {
            get { return urlGenerator; }
        }

        public QueryContext(
            IAppEntity app,
            IAssetRepository assetRepository,
            IContentQueryService contentQuery,
            IGraphQLUrlGenerator urlGenerator,
            ClaimsPrincipal user)
        {
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));

            this.assetRepository = assetRepository;
            this.contentQuery = contentQuery;
            this.urlGenerator = urlGenerator;

            this.user = user;

            this.app = app;
        }

        public async Task<IAssetEntity> FindAssetAsync(Guid id)
        {
            var asset = cachedAssets.GetOrDefault(id);

            if (asset == null)
            {
                asset = await assetRepository.FindAssetAsync(id).ConfigureAwait(false);

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
                content = (await contentQuery.FindContentAsync(app, schemaId.ToString(), user, id).ConfigureAwait(false)).Content;

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
            var contents = (await contentQuery.QueryWithCountAsync(app, schemaId.ToString(), user, false, null, query).ConfigureAwait(false)).Items;

            foreach (var content in contents)
            {
                cachedContents[content.Id] = content;
            }

            return contents;
        }

        public Task<IReadOnlyList<IAssetEntity>> GetReferencedAssetsAsync(JToken value)
        {
            var ids = ParseIds(value);

            return GetReferencedAssetsAsync(ids);
        }

        public async Task<IReadOnlyList<IAssetEntity>> GetReferencedAssetsAsync(ICollection<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedAssets = new HashSet<Guid>(ids.Where(id => !cachedAssets.ContainsKey(id)));

            if (notLoadedAssets.Count > 0)
            {
                var assets = await assetRepository.QueryAsync(app.Id, null, notLoadedAssets, null, int.MaxValue).ConfigureAwait(false);

                foreach (var asset in assets)
                {
                    cachedAssets[asset.Id] = asset;
                }
            }

            return ids.Select(id => cachedAssets.GetOrDefault(id)).Where(x => x != null).ToList();
        }

        public Task<IReadOnlyList<IContentEntity>> GetReferencedContentsAsync(Guid schemaId, JToken value)
        {
            var ids = ParseIds(value);

            return GetReferencedContentsAsync(schemaId, ids);
        }

        public async Task<IReadOnlyList<IContentEntity>> GetReferencedContentsAsync(Guid schemaId, ICollection<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedContents = new HashSet<Guid>(ids.Where(id => !cachedContents.ContainsKey(id)));

            if (notLoadedContents.Count > 0)
            {
                var contents = (await contentQuery.QueryWithCountAsync(app, schemaId.ToString(), user, false, notLoadedContents, null).ConfigureAwait(false)).Items;

                foreach (var content in contents)
                {
                    cachedContents[content.Id] = content;
                }
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
