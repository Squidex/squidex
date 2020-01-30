// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class ConvertData : IContentEnricherStep
    {
        private readonly IAssetUrlGenerator assetUrlGenerator;

        public ConvertData(IAssetUrlGenerator assetUrlGenerator)
        {
            Guard.NotNull(assetUrlGenerator);

            this.assetUrlGenerator = assetUrlGenerator;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
            var converters = GenerateConverters(context).ToArray();

            var resolveDataDraft = context.IsUnpublished() || context.IsFrontendClient;

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                var schema = await schemas(group.Key);

                foreach (var content in group)
                {
                    if (content.Data != null)
                    {
                        content.Data = content.Data.ConvertName2Name(schema.SchemaDef, converters);
                    }

                    if (content.DataDraft != null && resolveDataDraft)
                    {
                        content.DataDraft = content.DataDraft.ConvertName2Name(schema.SchemaDef, converters);
                    }
                    else
                    {
                        content.DataDraft = null!;
                    }
                }
            }
        }

        private IEnumerable<FieldConverter> GenerateConverters(Context context)
        {
            if (!context.IsFrontendClient)
            {
                yield return FieldConverters.ExcludeHidden();
                yield return FieldConverters.ForNestedName2Name(ValueConverters.ExcludeHidden());
            }

            yield return FieldConverters.ExcludeChangedTypes();
            yield return FieldConverters.ForNestedName2Name(ValueConverters.ExcludeChangedTypes());

            yield return FieldConverters.ResolveInvariant(context.App.LanguagesConfig);
            yield return FieldConverters.ResolveLanguages(context.App.LanguagesConfig);

            if (!context.IsFrontendClient)
            {
                if (!context.IsNoResolveLanguages())
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
