// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;

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

    public async Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        // Get the references across all references to reduce number of database calls.
        var referenceCleaner = await CleanReferencesAsync(context, contents, schemas, ct);

        // Get the fields, because they are the same for all schemas.
        var fieldNames = GetFieldNames(context);

        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            ct.ThrowIfCancellationRequested();

            var (schema, components) = await schemas(group.Key);

            // Reuse the converter for all contents of this schema.
            var converter = GenerateConverter(context, components, schema, fieldNames, referenceCleaner);

            foreach (var content in group)
            {
                content.Data = converter.Convert(content.Data);
            }
        }
    }

    private async Task<ValueReferencesConverter?> CleanReferencesAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        if (context.NoCleanup())
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
                    content.Data.AddReferencedIds(schema, ids, components);
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
        var result = await contentRepository.QueryIdsAsync(context.App, ids, context.Scope(), ct);

        return result.Select(x => x.Id);
    }

    private async Task<IEnumerable<DomainId>> QueryAssetIdsAsync(Context context, HashSet<DomainId> ids,
        CancellationToken ct)
    {
        var result = await assetRepository.QueryIdsAsync(context.App.Id, ids, ct);

        return result;
    }

    private ContentConverter GenerateConverter(Context context, ResolvedComponents components, Schema schema, HashSet<string>? fieldNames, ValueReferencesConverter? cleanReferences)
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

        converter.Add(new ResolveFromPreviousPartitioning(context.App.Languages));

        if (!context.IsFrontendClient && !context.NoDefaults())
        {
            converter.Add(new AddDefaultValues(context.App.PartitionResolver())
            {
                IgnoreNonMasterFields = true,
                IgnoreRequiredFields = false,
                // If field names are given we run the enrichment only on the specified fields.
                FieldNames = fieldNames
            });
        }

        converter.Add(
            new ResolveLanguages(
                context.App.Languages,
                context.Languages().ToArray())
            {
                ResolveFallback = !context.IsFrontendClient && !context.NoResolveLanguages(),
                // If field names are given we run the enrichment only on the specified fields.
                FieldNames = fieldNames
            });

        if (!context.IsFrontendClient)
        {
            var assetUrls = context.ResolveUrls().ToList();

            if (assetUrls.Count > 0)
            {
                converter.Add(new ResolveAssetUrls(context.App.NamedId(), urlGenerator, assetUrls));
            }
        }

        if (!context.IsFrontendClient || context.ResolveSchemaNames())
        {
            converter.Add(new AddSchemaNames(components));
        }

        return converter;
    }

    private static HashSet<string>? GetFieldNames(Context context)
    {
        var source = context.Fields();

        if (source is not { Count: > 0 })
        {
            return null;
        }

        var fields = new HashSet<string>();

        foreach (var field in source)
        {
            if (FieldNames.IsDataField(field, out var dataField))
            {
                fields.Add(dataField);
            }
            else
            {
                fields.Add(field);
            }
        }

        return fields.Count == 0 ? null : fields;
    }
}
