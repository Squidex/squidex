// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;

#pragma warning disable MA0073 // Avoid comparison with bool constant

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class ConvertData : IContentEnricherStep
{
    private readonly IUrlGenerator urlGenerator;
    private readonly IAssetRepository assetRepository;
    private readonly IContentRepository contentRepository;
    private readonly ExcludeChangedTypes excludeChangedTypes;

    public ConvertData(IUrlGenerator urlGenerator, IJsonSerializer serializer,
        IAssetRepository assetRepository, IContentRepository contentRepository)
    {
        this.urlGenerator = urlGenerator;
        this.assetRepository = assetRepository;
        this.contentRepository = contentRepository;

        excludeChangedTypes = new ExcludeChangedTypes(serializer);
    }

    public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        var referenceCleaner = await CleanReferencesAsync(context, contents, schemas, ct);

        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            ct.ThrowIfCancellationRequested();

            var (schema, components) = await schemas(group.Key);

            var converter = GenerateConverter(context, components, schema.SchemaDef, referenceCleaner);

            foreach (var content in group)
            {
                content.Data = converter.Convert(content.Data);
            }
        }
    }

    private async Task<ValueReferencesConverter?> CleanReferencesAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        if (context.ShouldSkipCleanup())
        {
            return null;
        }

        using (Telemetry.Activities.StartActivity("ConvertData/CleanReferencesAsync"))
        {
            var ids = new HashSet<DomainId>();

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                var (schema, components) = await schemas(group.Key);

                foreach (var content in group)
                {
                    content.Data.AddReferencedIds(schema.SchemaDef, ids, components);
                }
            }

            if (ids.Count > 0)
            {
                var (assets, refContents) = await AsyncHelper.WhenAll(
                    QueryAssetIdsAsync(context, ids, ct),
                    QueryContentIdsAsync(context, ids, ct));

                var foundIds = assets.Union(refContents).ToHashSet();

                return new ValueReferencesConverter(foundIds);
            }
        }

        return null;
    }

    private async Task<IEnumerable<DomainId>> QueryContentIdsAsync(Context context, HashSet<DomainId> ids,
        CancellationToken ct)
    {
        var result = await contentRepository.QueryIdsAsync(context.App.Id, ids, context.Scope(), ct);

        return result.Select(x => x.Id);
    }

    private async Task<IEnumerable<DomainId>> QueryAssetIdsAsync(Context context, HashSet<DomainId> ids,
        CancellationToken ct)
    {
        var result = await assetRepository.QueryIdsAsync(context.App.Id, ids, ct);

        return result;
    }

    private ContentConverter GenerateConverter(Context context, ResolvedComponents components, Schema schema, ValueReferencesConverter? cleanReferences)
    {
        var converter = new ContentConverter(components, schema);

        if (!context.IsFrontendClient)
        {
            converter.Add(ExcludeHidden.Instance);
        }

        converter.Add(excludeChangedTypes);

        if (cleanReferences != null)
        {
            converter.Add(cleanReferences);
        }

        converter.Add(new ResolveInvariant(context.App.Languages));

        converter.Add(
            new ResolveLanguages(context.App.Languages,
                context.IsFrontendClient == false &&
                context.ShouldResolveLanguages(),
                context.Languages().ToArray()));

        if (!context.IsFrontendClient)
        {
            var assetUrls = context.AssetUrls().ToList();

            if (assetUrls.Count > 0)
            {
                converter.Add(new ResolveAssetUrls(context.App.NamedId(), urlGenerator, assetUrls));
            }

            converter.Add(new AddSchemaNames(components));
        }

        return converter;
    }
}
