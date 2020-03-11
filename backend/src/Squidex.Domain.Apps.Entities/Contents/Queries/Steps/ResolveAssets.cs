// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class ResolveAssets : IContentEnricherStep
    {
        private static readonly ILookup<Guid, IEnrichedAssetEntity> EmptyAssets = Enumerable.Empty<IEnrichedAssetEntity>().ToLookup(x => x.Id);

        private readonly IUrlGenerator urlGenerator;
        private readonly IAssetQueryService assetQuery;
        private readonly IRequestCache requestCache;

        public ResolveAssets(IUrlGenerator urlGenerator, IAssetQueryService assetQuery, IRequestCache requestCache)
        {
            Guard.NotNull(urlGenerator);
            Guard.NotNull(assetQuery);
            Guard.NotNull(requestCache);

            this.urlGenerator = urlGenerator;
            this.assetQuery = assetQuery;
            this.requestCache = requestCache;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
            if (ShouldEnrich(context))
            {
                var ids = new HashSet<Guid>();

                foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
                {
                    var schema = await schemas(group.Key);

                    AddAssetIds(ids, schema, group);
                }

                var assets = await GetAssetsAsync(context, ids);

                foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
                {
                    var schema = await schemas(group.Key);

                    ResolveAssetsUrls(schema, group, assets);
                }
            }
        }

        private void ResolveAssetsUrls(ISchemaEntity schema, IGrouping<Guid, ContentEntity> contents, ILookup<Guid, IEnrichedAssetEntity> assets)
        {
            foreach (var field in schema.SchemaDef.ResolvingAssets())
            {
                foreach (var content in contents)
                {
                    if (content.ReferenceData == null)
                    {
                        content.ReferenceData = new NamedContentData();
                    }

                    var fieldReference = content.ReferenceData.GetOrAdd(field.Name, _ => new ContentFieldData())!;

                    if (content.Data.TryGetValue(field.Name, out var fieldData) && fieldData != null)
                    {
                        foreach (var (partitionKey, partitionValue) in fieldData)
                        {
                            var referencedAsset =
                                field.GetReferencedIds(partitionValue)
                                    .Select(x => assets[x])
                                    .SelectMany(x => x)
                                    .FirstOrDefault();

                            if (referencedAsset != null)
                            {
                                IJsonValue array;

                                if (referencedAsset.Type == AssetType.Image)
                                {
                                    var url = urlGenerator.AssetContent(Guid.Parse(referencedAsset.Id.ToString()));

                                    array = JsonValue.Array(url, referencedAsset.FileName);
                                }
                                else
                                {
                                    array = JsonValue.Array(referencedAsset.FileName);
                                }

                                requestCache.AddDependency(referencedAsset.Id, referencedAsset.Version);

                                fieldReference.AddJsonValue(partitionKey, array);
                            }
                        }
                    }
                }
            }
        }

        private async Task<ILookup<Guid, IEnrichedAssetEntity>> GetAssetsAsync(Context context, HashSet<Guid> ids)
        {
            if (ids.Count == 0)
            {
                return EmptyAssets;
            }

            var assets = await assetQuery.QueryAsync(context.Clone().WithoutAssetEnrichment(true), null, Q.Empty.WithIds(ids));

            return assets.ToLookup(x => x.Id);
        }

        private void AddAssetIds(HashSet<Guid> ids, ISchemaEntity schema, IEnumerable<ContentEntity> contents)
        {
            foreach (var content in contents)
            {
                content.Data.AddReferencedIds(schema.SchemaDef.ResolvingAssets(), ids, 1);
            }
        }

        private static bool ShouldEnrich(Context context)
        {
            return context.IsFrontendClient && context.ShouldEnrichContent();
        }
    }
}
