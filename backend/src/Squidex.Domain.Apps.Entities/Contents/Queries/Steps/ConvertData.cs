// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class ConvertData : IContentEnricherStep
    {
        private readonly IUrlGenerator urlGenerator;
        private readonly IJsonSerializer jsonSerializer;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;
        private readonly FieldConverter excludedChangedField;
        private readonly FieldConverter excludedHiddenField;

        public ConvertData(IUrlGenerator urlGenerator, IJsonSerializer jsonSerializer,
            IAssetRepository assetRepository, IContentRepository contentRepository)
        {
            this.urlGenerator = urlGenerator;
            this.jsonSerializer = jsonSerializer;
            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;

            excludedChangedField = FieldConverters.ExcludeChangedTypes(jsonSerializer);
            excludedHiddenField = FieldConverters.ExcludeHidden;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
            CancellationToken ct)
        {
            var referenceCleaner = await CleanReferencesAsync(context, contents, schemas, ct);

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                ct.ThrowIfCancellationRequested();

                var (schema, components) = await schemas(group.Key);

                var converters = GenerateConverters(context, components, referenceCleaner).ToArray();

                foreach (var content in group)
                {
                    content.Data = content.Data.Convert(schema.SchemaDef, converters);
                }
            }
        }

        private async Task<ValueConverter?> CleanReferencesAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
            CancellationToken ct)
        {
            if (!context.ShouldSkipCleanup())
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

                    return ValueReferencesConverter.CleanReferences(foundIds);
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

        private IEnumerable<FieldConverter> GenerateConverters(Context context, ResolvedComponents components, ValueConverter? cleanReferences)
        {
            if (!context.IsFrontendClient)
            {
                yield return excludedHiddenField;
                yield return FieldConverters.ForValues(components, ValueConverters.ExcludeHidden);
            }

            yield return excludedChangedField;
            yield return FieldConverters.ForValues(components, ValueConverters.ExcludeChangedTypes(jsonSerializer));

            if (cleanReferences != null)
            {
                yield return FieldConverters.ForValues(components, cleanReferences);
            }

            yield return FieldConverters.ResolveInvariant(context.App.Languages);
            yield return FieldConverters.ResolveLanguages(context.App.Languages);

            if (!context.IsFrontendClient)
            {
                if (context.ShouldResolveLanguages())
                {
                    yield return FieldConverters.ResolveFallbackLanguages(context.App.Languages);
                }

                var languages = context.Languages();

                if (languages.Any())
                {
                    yield return FieldConverters.FilterLanguages(context.App.Languages, languages);
                }

                var assetUrls = context.AssetUrls().ToList();

                if (assetUrls.Count > 0)
                {
                    var appId = context.App.NamedId();

                    var resolveAssetUrls = ValueConverters.ResolveAssetUrls(appId, assetUrls, urlGenerator);

                    yield return FieldConverters.ForValues(components, resolveAssetUrls);
                }
            }
        }
    }
}
