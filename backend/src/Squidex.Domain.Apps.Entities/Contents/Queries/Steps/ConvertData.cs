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
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class ConvertData : IContentEnricherStep
    {
        private readonly IAssetUrlGenerator assetUrlGenerator;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;

        public ConvertData(IAssetUrlGenerator assetUrlGenerator, IAssetRepository assetRepository, IContentRepository contentRepository)
        {
            Guard.NotNull(assetUrlGenerator);
            Guard.NotNull(assetRepository);
            Guard.NotNull(contentRepository);

            this.assetUrlGenerator = assetUrlGenerator;
            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
            var resolveDataDraft = context.ShouldProvideUnpublished() || context.IsFrontendClient;

            var referenceCleaner = await CleanReferencesAsync(context, contents, schemas);

            var converters = GenerateConverters(context, referenceCleaner).ToArray();

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                var schema = await schemas(group.Key);

                foreach (var content in group)
                {
                    content.Data = content.Data.ConvertName2Name(schema.SchemaDef, converters);
                }
            }
        }

        private async Task<ValueConverter?> CleanReferencesAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
            if (context.ShouldCleanup())
            {
                var ids = new HashSet<Guid>();

                foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
                {
                    var schema = await schemas(group.Key);

                    foreach (var content in group)
                    {
                        content.Data.AddReferencedIds(schema.SchemaDef, ids);
                    }
                }

                if (ids.Count > 0)
                {
                    var taskForAssets = QueryAssetIdsAsync(context, ids);
                    var taskForContents = QueryContentIdsAsync(context, ids);

                    await Task.WhenAll(taskForAssets, taskForContents);

                    var foundIds = new HashSet<Guid>(taskForAssets.Result.Union(taskForContents.Result));

                    return ValueReferencesConverter.CleanReferences(foundIds);
                }
            }

            return null;
        }

        private async Task<IEnumerable<Guid>> QueryContentIdsAsync(Context context, HashSet<Guid> ids)
        {
            var result = await contentRepository.QueryIdsAsync(context.App.Id, ids, context.ShouldProvideUnpublished() ? SearchScope.All : SearchScope.Published);

            return result.Select(x => x.Id);
        }

        private async Task<IEnumerable<Guid>> QueryAssetIdsAsync(Context context, HashSet<Guid> ids)
        {
            var result = await assetRepository.QueryIdsAsync(context.App.Id, ids);

            return result;
        }

        private IEnumerable<FieldConverter> GenerateConverters(Context context, ValueConverter? cleanReferences)
        {
            if (!context.IsFrontendClient)
            {
                yield return FieldConverters.ExcludeHidden();
                yield return FieldConverters.ForNestedName2Name(ValueConverters.ExcludeHidden());
            }

            yield return FieldConverters.ExcludeChangedTypes();
            yield return FieldConverters.ForNestedName2Name(ValueConverters.ExcludeChangedTypes());

            if (cleanReferences != null)
            {
                yield return FieldConverters.ForValues(cleanReferences);
                yield return FieldConverters.ForNestedName2Name(cleanReferences);
            }

            yield return FieldConverters.ResolveInvariant(context.App.LanguagesConfig);
            yield return FieldConverters.ResolveLanguages(context.App.LanguagesConfig);

            if (!context.IsFrontendClient)
            {
                if (context.ShouldResolveLanguages())
                {
                    yield return FieldConverters.ResolveFallbackLanguages(context.App.LanguagesConfig);
                }

                var languages = context.Languages();

                if (languages.Any())
                {
                    yield return FieldConverters.FilterLanguages(context.App.LanguagesConfig, languages);
                }

                var assetUrls = context.AssetUrls();

                if (assetUrls.Any())
                {
                    yield return FieldConverters.ResolveAssetUrls(assetUrls.ToList(), assetUrlGenerator);
                }
            }
        }
    }
}
