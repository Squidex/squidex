// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class ConvertData : IContentEnricherStep
    {
        private readonly IUrlGenerator urlGenerator;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;

        public ConvertData(IUrlGenerator urlGenerator, IAssetRepository assetRepository, IContentRepository contentRepository)
        {
            Guard.NotNull(urlGenerator, nameof(urlGenerator));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.urlGenerator = urlGenerator;
            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
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
                var ids = new HashSet<DomainId>();

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
                    var (assets, refContents) = await AsyncHelper.WhenAll(
                        QueryAssetIdsAsync(context, ids),
                        QueryContentIdsAsync(context, ids));

                    var foundIds = assets.Union(refContents).ToHashSet();

                    return ValueReferencesConverter.CleanReferences(foundIds);
                }
            }

            return null;
        }

        private async Task<IEnumerable<DomainId>> QueryContentIdsAsync(Context context, HashSet<DomainId> ids)
        {
            var result = await contentRepository.QueryIdsAsync(context.App.Id, ids, context.Scope());

            return result.Select(x => x.Id);
        }

        private async Task<IEnumerable<DomainId>> QueryAssetIdsAsync(Context context, HashSet<DomainId> ids)
        {
            var result = await assetRepository.QueryIdsAsync(context.App.Id, ids);

            return result;
        }

        private IEnumerable<FieldConverter> GenerateConverters(Context context, ValueConverter? cleanReferences)
        {
            if (!context.IsFrontendClient)
            {
                yield return FieldConverters.ExcludeHidden;
                yield return FieldConverters.ForValues(ValueConverters.ForNested(ValueConverters.ExcludeHidden));
            }

            yield return FieldConverters.ExcludeChangedTypes;
            yield return FieldConverters.ForValues(ValueConverters.ForNested(ValueConverters.ExcludeChangedTypes));

            if (cleanReferences != null)
            {
                yield return FieldConverters.ForValues(cleanReferences);
                yield return FieldConverters.ForValues(ValueConverters.ForNested(cleanReferences));
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

                    yield return FieldConverters.ForValues(resolveAssetUrls);
                    yield return FieldConverters.ForValues(ValueConverters.ForNested(resolveAssetUrls));
                }
            }
        }
    }
}
