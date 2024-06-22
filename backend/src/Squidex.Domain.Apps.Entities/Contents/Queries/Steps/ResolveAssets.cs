// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class ResolveAssets : IContentEnricherStep
{
    private static readonly ILookup<DomainId, EnrichedAsset> EmptyAssets = Enumerable.Empty<EnrichedAsset>().ToLookup(x => x.Id);

    private readonly IUrlGenerator urlGenerator;
    private readonly IAssetQueryService assetQuery;
    private readonly IRequestCache requestCache;

    public ResolveAssets(IUrlGenerator urlGenerator, IAssetQueryService assetQuery, IRequestCache requestCache)
    {
        this.urlGenerator = urlGenerator;
        this.assetQuery = assetQuery;
        this.requestCache = requestCache;
    }

    public async Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        if (!ShouldEnrich(context))
        {
            return;
        }

        var ids = new HashSet<DomainId>();

        // Group by schema, so we only fetch the schema once.
        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            var (schema, components) = await schemas(group.Key);

            AddAssetIds(ids, schema, components, group);
        }

        var assets = await GetAssetsAsync(context, ids, ct);

        // Group by schema, so we only fetch the schema once.
        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            var (schema, components) = await schemas(group.Key);

            ResolveAssetsUrls(schema, components, group, assets);
        }
    }

    private void ResolveAssetsUrls(Schema schema, ResolvedComponents components,
        IGrouping<DomainId, EnrichedContent> contents, ILookup<DomainId, EnrichedAsset> assets)
    {
        HashSet<DomainId>? fieldIds = null;

        foreach (var field in schema.ResolvingAssets())
        {
            foreach (var content in contents)
            {
                content.ReferenceData ??= [];

                var fieldReference = content.ReferenceData.GetOrAdd(field.Name, _ => [])!;

                if (content.Data.TryGetValue(field.Name, out var fieldData) && fieldData != null)
                {
                    foreach (var (partitionKey, partitionValue) in fieldData)
                    {
                        fieldIds ??= [];
                        fieldIds.Clear();

                        partitionValue.AddReferencedIds(field, fieldIds, components);

                        var referencedAsset =
                            fieldIds
                                .Select(x => assets[x])
                                .SelectMany(x => x)
                                .FirstOrDefault();

                        if (referencedAsset != null)
                        {
                            var array = new JsonArray();

                            if (IsImage(referencedAsset))
                            {
                                var url = urlGenerator.AssetContent(
                                    referencedAsset.AppId,
                                    referencedAsset.Id.ToString());

                                array.Add(url);
                            }

                            array.Add(referencedAsset.FileName);

                            requestCache.AddDependency(referencedAsset.UniqueId, referencedAsset.Version);

                            fieldReference.AddLocalized(partitionKey, array);
                        }
                    }
                }
            }
        }
    }

    private static bool IsImage(EnrichedAsset asset)
    {
        const int PreviewLimit = 10 * 1024;

        return asset.Type == AssetType.Image || (asset.MimeType == "image/svg+xml" && asset.FileSize < PreviewLimit);
    }

    private async Task<ILookup<DomainId, EnrichedAsset>> GetAssetsAsync(Context context, HashSet<DomainId> ids,
        CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return EmptyAssets;
        }

        var queryContext = context.Clone(b => b
            .WithNoAssetEnrichment(true)
            .WithNoTotal());

        var assets = await assetQuery.QueryAsync(queryContext, null, Q.Empty.WithIds(ids), ct);

        return assets.ToLookup(x => x.Id);
    }

    private static void AddAssetIds(HashSet<DomainId> ids, Schema schema, ResolvedComponents components, IEnumerable<EnrichedContent> contents)
    {
        foreach (var content in contents)
        {
            content.Data.AddReferencedIds(schema.ResolvingAssets(), ids, components, 1);
        }
    }

    private static bool ShouldEnrich(Context context)
    {
        return context.IsFrontendClient && !context.NoEnrichment();
    }
}
