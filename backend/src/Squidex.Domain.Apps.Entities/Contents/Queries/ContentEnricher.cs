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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentEnricher : IContentEnricher
    {
        private const string DefaultColor = StatusColors.Draft;
        private static readonly ILookup<Guid, IEnrichedContentEntity> EmptyContents = Enumerable.Empty<IEnrichedContentEntity>().ToLookup(x => x.Id);
        private static readonly ILookup<Guid, IEnrichedAssetEntity> EmptyAssets = Enumerable.Empty<IEnrichedAssetEntity>().ToLookup(x => x.Id);
        private readonly IAssetQueryService assetQuery;
        private readonly IAssetUrlGenerator assetUrlGenerator;
        private readonly Lazy<IContentQueryService> contentQuery;
        private readonly IContentWorkflow contentWorkflow;

        private IContentQueryService ContentQuery
        {
            get { return contentQuery.Value; }
        }

        public ContentEnricher(IAssetQueryService assetQuery, IAssetUrlGenerator assetUrlGenerator, Lazy<IContentQueryService> contentQuery, IContentWorkflow contentWorkflow)
        {
            Guard.NotNull(assetQuery);
            Guard.NotNull(assetUrlGenerator);
            Guard.NotNull(contentQuery);
            Guard.NotNull(contentWorkflow);

            this.assetQuery = assetQuery;
            this.assetUrlGenerator = assetUrlGenerator;
            this.contentQuery = contentQuery;
            this.contentWorkflow = contentWorkflow;
        }

        public async Task<IEnrichedContentEntity> EnrichAsync(IContentEntity content, Context context)
        {
            Guard.NotNull(content);

            var enriched = await EnrichAsync(Enumerable.Repeat(content, 1), context);

            return enriched[0];
        }

        public async Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents, Context context)
        {
            Guard.NotNull(contents);
            Guard.NotNull(context);

            using (Profiler.TraceMethod<ContentEnricher>())
            {
                var results = new List<ContentEntity>();

                if (contents.Any())
                {
                    var appVersion = context.App.Version;

                    var cache = new Dictionary<(Guid, Status), StatusInfo>();

                    foreach (var content in contents)
                    {
                        var result = SimpleMapper.Map(content, new ContentEntity());

                        await EnrichColorAsync(content, result, cache);

                        if (ShouldEnrichWithStatuses(context))
                        {
                            await EnrichNextsAsync(content, result, context);
                            await EnrichCanUpdateAsync(content, result);
                        }

                        results.Add(result);
                    }

                    foreach (var group in results.GroupBy(x => x.SchemaId.Id))
                    {
                        var schema = await ContentQuery.GetSchemaOrThrowAsync(context, group.Key.ToString());

                        foreach (var content in group)
                        {
                            content.CacheDependencies = new HashSet<object?>
                            {
                                schema.Id,
                                schema.Version
                            };
                        }

                        if (ShouldEnrichWithSchema(context))
                        {
                            var referenceFields = schema.SchemaDef.ReferenceFields().ToArray();

                            var schemaName = schema.SchemaDef.Name;
                            var schemaDisplayName = schema.SchemaDef.DisplayNameUnchanged();

                            foreach (var content in group)
                            {
                                content.ReferenceFields = referenceFields;
                                content.SchemaName = schemaName;
                                content.SchemaDisplayName = schemaDisplayName;
                            }
                        }
                    }

                    if (ShouldEnrich(context))
                    {
                        await EnrichReferencesAsync(context, results);
                        await EnrichAssetsAsync(context, results);
                    }
                }

                return results;
            }
        }

        private async Task EnrichAssetsAsync(Context context, List<ContentEntity> contents)
        {
            var ids = new HashSet<Guid>();

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                var schema = await ContentQuery.GetSchemaOrThrowAsync(context, group.Key.ToString());

                AddAssetIds(ids, schema, group);
            }

            var assets = await GetAssetsAsync(context, ids);

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                var schema = await ContentQuery.GetSchemaOrThrowAsync(context, group.Key.ToString());

                ResolveAssets(schema, group, assets);
            }
        }

        private async Task EnrichReferencesAsync(Context context, List<ContentEntity> contents)
        {
            var ids = new HashSet<Guid>();

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                var schema = await ContentQuery.GetSchemaOrThrowAsync(context, group.Key.ToString());

                AddReferenceIds(ids, schema, group);
            }

            var references = await GetReferencesAsync(context, ids);

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                var schema = await ContentQuery.GetSchemaOrThrowAsync(context, group.Key.ToString());

                await ResolveReferencesAsync(context, schema, group, references);
            }
        }

        private async Task ResolveReferencesAsync(Context context, ISchemaEntity schema, IEnumerable<ContentEntity> contents, ILookup<Guid, IEnrichedContentEntity> references)
        {
            var formatted = new Dictionary<IContentEntity, JsonObject>();

            foreach (var field in schema.SchemaDef.ResolvingReferences())
            {
                foreach (var content in contents)
                {
                    if (content.ReferenceData == null)
                    {
                        content.ReferenceData = new NamedContentData();
                    }

                    var fieldReference = content.ReferenceData.GetOrAdd(field.Name, _ => new ContentFieldData())!;

                    try
                    {
                        if (content.DataDraft.TryGetValue(field.Name, out var fieldData) && fieldData != null)
                        {
                            foreach (var partitionValue in fieldData)
                            {
                                var referencedContents =
                                    field.GetReferencedIds(partitionValue.Value, Ids.ContentOnly)
                                        .Select(x => references[x])
                                        .SelectMany(x => x)
                                        .ToList();

                                if (referencedContents.Count == 1)
                                {
                                    var reference = referencedContents[0];

                                    var referencedSchema = await ContentQuery.GetSchemaOrThrowAsync(context, reference.SchemaId.Id.ToString());

                                    content.CacheDependencies.Add(referencedSchema.Id);
                                    content.CacheDependencies.Add(referencedSchema.Version);
                                    content.CacheDependencies.Add(reference.Id);
                                    content.CacheDependencies.Add(reference.Version);

                                    var value = formatted.GetOrAdd(reference, x => Format(x, context, referencedSchema));

                                    fieldReference.AddJsonValue(partitionValue.Key, value);
                                }
                                else if (referencedContents.Count > 1)
                                {
                                    var value = CreateFallback(context, referencedContents);

                                    fieldReference.AddJsonValue(partitionValue.Key, value);
                                }
                            }
                        }
                    }
                    catch (DomainObjectNotFoundException)
                    {
                        continue;
                    }
                }
            }
        }

        private void ResolveAssets(ISchemaEntity schema, IGrouping<Guid, ContentEntity> contents, ILookup<Guid, IEnrichedAssetEntity> assets)
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

                    if (content.DataDraft.TryGetValue(field.Name, out var fieldData) && fieldData != null)
                    {
                        foreach (var partitionValue in fieldData)
                        {
                            var referencedImage =
                                field.GetReferencedIds(partitionValue.Value, Ids.ContentOnly)
                                    .Select(x => assets[x])
                                    .SelectMany(x => x)
                                    .FirstOrDefault(x => x.IsImage);

                            if (referencedImage != null)
                            {
                                var url = assetUrlGenerator.GenerateUrl(referencedImage.Id.ToString());

                                content.CacheDependencies.Add(referencedImage.Id);
                                content.CacheDependencies.Add(referencedImage.Version);

                                fieldReference.AddJsonValue(partitionValue.Key, JsonValue.Create(url));
                            }
                        }
                    }
                }
            }
        }

        private static JsonObject Format(IContentEntity content, Context context, ISchemaEntity referencedSchema)
        {
            return content.DataDraft.FormatReferences(referencedSchema.SchemaDef, context.App.LanguagesConfig);
        }

        private static JsonObject CreateFallback(Context context, List<IEnrichedContentEntity> referencedContents)
        {
            var text = $"{referencedContents.Count} Reference(s)";

            var value = JsonValue.Object();

            foreach (var language in context.App.LanguagesConfig)
            {
                value.Add(language.Key, text);
            }

            return value;
        }

        private void AddReferenceIds(HashSet<Guid> ids, ISchemaEntity schema, IEnumerable<ContentEntity> contents)
        {
            foreach (var content in contents)
            {
                ids.AddRange(content.DataDraft.GetReferencedIds(schema.SchemaDef.ResolvingReferences(), Ids.ContentOnly));
            }
        }

        private void AddAssetIds(HashSet<Guid> ids, ISchemaEntity schema, IEnumerable<ContentEntity> contents)
        {
            foreach (var content in contents)
            {
                ids.AddRange(content.DataDraft.GetReferencedIds(schema.SchemaDef.ResolvingAssets(), Ids.ContentOnly));
            }
        }

        private async Task<ILookup<Guid, IEnrichedContentEntity>> GetReferencesAsync(Context context, HashSet<Guid> ids)
        {
            if (ids.Count == 0)
            {
                return EmptyContents;
            }

            var references = await ContentQuery.QueryAsync(context.Clone().WithNoContentEnrichment(true), ids.ToList());

            return references.ToLookup(x => x.Id);
        }

        private async Task<ILookup<Guid, IEnrichedAssetEntity>> GetAssetsAsync(Context context, HashSet<Guid> ids)
        {
            if (ids.Count == 0)
            {
                return EmptyAssets;
            }

            var assets = await assetQuery.QueryAsync(context.Clone().WithNoAssetEnrichment(true), Q.Empty.WithIds(ids));

            return assets.ToLookup(x => x.Id);
        }

        private async Task EnrichCanUpdateAsync(IContentEntity content, ContentEntity result)
        {
            result.CanUpdate = await contentWorkflow.CanUpdateAsync(content);
        }

        private async Task EnrichNextsAsync(IContentEntity content, ContentEntity result, Context context)
        {
            result.Nexts = await contentWorkflow.GetNextsAsync(content, context.User);
        }

        private async Task EnrichColorAsync(IContentEntity content, ContentEntity result, Dictionary<(Guid, Status), StatusInfo> cache)
        {
            result.StatusColor = await GetColorAsync(content, cache);
        }

        private async Task<string> GetColorAsync(IContentEntity content, Dictionary<(Guid, Status), StatusInfo> cache)
        {
            if (!cache.TryGetValue((content.SchemaId.Id, content.Status), out var info))
            {
                info = await contentWorkflow.GetInfoAsync(content);

                if (info == null)
                {
                    info = new StatusInfo(content.Status, DefaultColor);
                }

                cache[(content.SchemaId.Id, content.Status)] = info;
            }

            return info.Color;
        }

        private static bool ShouldEnrichWithSchema(Context context)
        {
            return context.IsFrontendClient;
        }

        private static bool ShouldEnrichWithStatuses(Context context)
        {
            return context.IsFrontendClient || context.IsResolveFlow();
        }

        private static bool ShouldEnrich(Context context)
        {
            return context.IsFrontendClient && !context.IsNoEnrichment();
        }
    }
}
