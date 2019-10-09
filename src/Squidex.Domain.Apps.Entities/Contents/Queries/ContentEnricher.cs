﻿// ==========================================================================
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
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
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
        private static readonly ILookup<Guid, IEnrichedContentEntity> EmptyReferences = Enumerable.Empty<IEnrichedContentEntity>().ToLookup(x => x.Id);
        private readonly Lazy<IContentQueryService> contentQuery;
        private readonly IContentWorkflow contentWorkflow;

        private IContentQueryService ContentQuery
        {
            get { return contentQuery.Value; }
        }

        public ContentEnricher(Lazy<IContentQueryService> contentQuery, IContentWorkflow contentWorkflow)
        {
            Guard.NotNull(contentQuery);
            Guard.NotNull(contentWorkflow);

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

                        await ResolveColorAsync(content, result, cache);

                        if (ShouldEnrichWithStatuses(context))
                        {
                            await ResolveNextsAsync(content, result, context);
                            await ResolveCanUpdateAsync(content, result);
                        }

                        results.Add(result);
                    }

                    foreach (var group in results.GroupBy(x => x.SchemaId.Id))
                    {
                        var schema = await ContentQuery.GetSchemaOrThrowAsync(context, group.Key.ToString());

                        foreach (var content in group)
                        {
                            content.CacheDependencies = new HashSet<object>
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

                        if (ShouldEnrich(context))
                        {
                            await ResolveReferencesAsync(schema, group, context);
                        }
                    }
                }

                return results;
            }
        }

        private async Task ResolveReferencesAsync(ISchemaEntity schema, IEnumerable<ContentEntity> contents, Context context)
        {
            var references = await GetReferencesAsync(schema, contents, context);

            var formatted = new Dictionary<IContentEntity, JsonObject>();

            foreach (var field in schema.SchemaDef.ResolvingReferences())
            {
                foreach (var content in contents)
                {
                    if (content.ReferenceData == null)
                    {
                        content.ReferenceData = new NamedContentData();
                    }

                    content.ReferenceData.GetOrAdd(field.Name, _ => new ContentFieldData());
                }

                try
                {
                    foreach (var content in contents)
                    {
                        var fieldReference = content.ReferenceData![field.Name];

                        if (fieldReference != null && content.DataDraft!.TryGetValue(field.Name, out var fieldData) && fieldData != null)
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
                }
                catch (DomainObjectNotFoundException)
                {
                    continue;
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

        private async Task<ILookup<Guid, IEnrichedContentEntity>> GetReferencesAsync(ISchemaEntity schema, IEnumerable<ContentEntity> contents, Context context)
        {
            var ids = new HashSet<Guid>();

            foreach (var content in contents)
            {
                ids.AddRange(content.DataDraft!.GetReferencedIds(schema.SchemaDef.ResolvingReferences(), Ids.ContentOnly));
            }

            if (ids.Count > 0)
            {
                var references = await ContentQuery.QueryAsync(context.Clone().WithNoEnrichment(true), ids.ToList());

                return references.ToLookup(x => x.Id);
            }
            else
            {
                return EmptyReferences;
            }
        }

        private async Task ResolveCanUpdateAsync(IContentEntity content, ContentEntity result)
        {
            result.CanUpdate = await contentWorkflow.CanUpdateAsync(content);
        }

        private async Task ResolveNextsAsync(IContentEntity content, ContentEntity result, Context context)
        {
            result.Nexts = await contentWorkflow.GetNextsAsync(content, context.User);
        }

        private async Task ResolveColorAsync(IContentEntity content, ContentEntity result, Dictionary<(Guid, Status), StatusInfo> cache)
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
