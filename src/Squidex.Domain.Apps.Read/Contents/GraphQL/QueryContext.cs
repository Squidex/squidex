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
using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Schemas.Services;

// ReSharper disable InvertIf

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public sealed class QueryContext
    {
        private readonly ConcurrentDictionary<Guid, IContentEntity> cachedContents = new ConcurrentDictionary<Guid, IContentEntity>();
        private readonly ConcurrentDictionary<Guid, IAssetEntity> cachedAssets = new ConcurrentDictionary<Guid, IAssetEntity>();
        private readonly IContentRepository contentRepository;
        private readonly IAssetRepository assetRepository;
        private readonly IGraphQLUrlGenerator urlGenerator;
        private readonly IScriptEngine scriptEngine;
        private readonly ISchemaProvider schemas;
        private readonly IAppEntity app;
        private readonly ClaimsPrincipal user;

        public IGraphQLUrlGenerator UrlGenerator
        {
            get { return urlGenerator; }
        }

        public QueryContext(
            IAppEntity app,
            IAssetRepository assetRepository,
            IContentRepository contentRepository,
            IGraphQLUrlGenerator urlGenerator,
            ISchemaProvider schemas,
            IScriptEngine scriptEngine,
            ClaimsPrincipal user)
        {
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(schemas, nameof(schemas));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));
            Guard.NotNull(user, nameof(user));
            Guard.NotNull(app, nameof(app));

            this.contentRepository = contentRepository;
            this.assetRepository = assetRepository;
            this.schemas = schemas;
            this.scriptEngine = scriptEngine;
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
                var schema = await schemas.FindSchemaByIdAsync(schemaId).ConfigureAwait(false);

                if (schema != null)
                {
                    content = await contentRepository.FindContentAsync(app, schemaId, id).ConfigureAwait(false);

                    if (content != null)
                    {
                        content.Data = scriptEngine.Transform(new ScriptContext { Data = content.Data, ContentId = content.Id, User = user }, schema.ScriptQuery);

                        cachedContents[content.Id] = content;
                    }
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
            var result = new List<IContentEntity>();

            var schema = await schemas.FindSchemaByIdAsync(schemaId).ConfigureAwait(false);

            if (schema != null)
            {
                result.AddRange(await contentRepository.QueryAsync(app, schemaId, false, null, query).ConfigureAwait(false));

                foreach (var content in result)
                {
                    content.Data = scriptEngine.Transform(new ScriptContext { Data = content.Data, ContentId = content.Id, User = user }, schema.ScriptQuery);

                    cachedContents[content.Id] = content;
                }
            }

            return result;
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
            var ids =  ParseIds(value);

            return GetReferencedContentsAsync(schemaId, ids);
        }

        public async Task<IReadOnlyList<IContentEntity>> GetReferencedContentsAsync(Guid schemaId, ICollection<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedContents = new HashSet<Guid>(ids.Where(id => !cachedContents.ContainsKey(id)));

            if (notLoadedContents.Count > 0)
            {
                var schema = await schemas.FindSchemaByIdAsync(schemaId).ConfigureAwait(false);

                if (schema != null)
                {
                    var contents = await contentRepository.QueryAsync(app, schemaId, false, notLoadedContents, null).ConfigureAwait(false);

                    foreach (var content in contents)
                    {
                        content.Data = scriptEngine.Transform(new ScriptContext { Data = content.Data, ContentId = content.Id, User = user }, schema.ScriptQuery);

                        cachedContents[content.Id] = content;
                    }
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
