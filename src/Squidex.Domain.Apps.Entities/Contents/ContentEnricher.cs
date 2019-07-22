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
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentEnricher : IContentEnricher
    {
        private const string DefaultColor = StatusColors.Draft;
        private static readonly Dictionary<Guid, IEnrichedContentEntity> EmptyReferences = new Dictionary<Guid, IEnrichedContentEntity>();
        private readonly Lazy<IContentQueryService> contentQuery;
        private readonly IContentWorkflow contentWorkflow;

        private IContentQueryService ContentQuery
        {
            get { return contentQuery.Value; }
        }

        public ContentEnricher(Lazy<IContentQueryService> contentQuery, IContentWorkflow contentWorkflow)
        {
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(contentWorkflow, nameof(contentWorkflow));

            this.contentQuery = contentQuery;
            this.contentWorkflow = contentWorkflow;
        }

        public async Task<IEnrichedContentEntity> EnrichAsync(IContentEntity content, Context context)
        {
            Guard.NotNull(content, nameof(content));

            var enriched = await EnrichAsync(Enumerable.Repeat(content, 1), context);

            return enriched[0];
        }

        public async Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents, Context context)
        {
            Guard.NotNull(contents, nameof(contents));
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<ContentEnricher>())
            {
                var results = new List<ContentEntity>();

                if (contents.Any())
                {
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

                    if (ShouldEnrichWithReferences(context))
                    {
                        foreach (var group in results.GroupBy(x => x.SchemaId.Id))
                        {
                            await ResolveReferencesAsync(group.Key, group, context);
                        }
                    }
                }

                return results;
            }
        }

        private async Task ResolveReferencesAsync(Guid schemaId, IEnumerable<ContentEntity> contents, Context context)
        {
            var appId = contents.First().AppId.Id;

            var schema = await ContentQuery.GetSchemaOrThrowAsync(context, schemaId.ToString());

            var referenceFields =
                schema.SchemaDef.Fields.OfType<IField<ReferencesFieldProperties>>()
                    .Where(x =>
                        x.Properties.SchemaId != Guid.Empty &&
                        x.Properties.MinItems == 1 &&
                        x.Properties.MaxItems == 1 &&
                        x.Properties.IsListField);

            var formatted = new Dictionary<Guid, JsonObject>();

            foreach (var field in referenceFields)
            {
                foreach (var content in contents)
                {
                    content.ReferenceData = new NamedContentData();
                    content.ReferenceData.GetOrAddNew(field.Name);
                }

                try
                {
                    var referencedSchemaId = field.Properties.SchemaId;
                    var referencedSchema = await ContentQuery.GetSchemaOrThrowAsync(context, referencedSchemaId.ToString());

                    var references = await GetReferencesAsync(referencedSchemaId, contents, field, context);

                    foreach (var content in contents)
                    {
                        var fieldReference = content.ReferenceData[field.Name];

                        if (content.DataDraft.TryGetValue(field.Name, out var fieldData))
                        {
                            foreach (var partition in fieldData)
                            {
                                var id = field.GetReferencedIds(partition.Value, Ids.ContentOnly).FirstOrDefault();

                                if (references.TryGetValue(id, out var reference))
                                {
                                    var value =
                                        formatted.GetOrAdd(id,
                                            _ => reference.DataDraft.FormatReferences(referencedSchema.SchemaDef, context.App.LanguagesConfig));

                                    fieldReference[partition.Key] = JsonValue.Create(value);
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

        private async Task<Dictionary<Guid, IEnrichedContentEntity>> GetReferencesAsync(Guid schemaId, IEnumerable<ContentEntity> contents, IField field, Context context)
        {
            var ids = new HashSet<Guid>();

            foreach (var content in contents)
            {
                ids.AddRange(content.DataDraft.GetReferencedIds(field, Ids.ContentOnly));
            }

            if (ids.Count > 0)
            {
                var references = await ContentQuery.QueryAsync(context.Clone().WithNoEnrichment(true), schemaId.ToString(), Q.Empty.WithIds(ids));

                return references.ToDictionary(x => x.Id);
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

        private static bool ShouldEnrichWithStatuses(Context context)
        {
            return context.IsFrontendClient || context.IsResolveFlow();
        }

        private static bool ShouldEnrichWithReferences(Context context)
        {
            return context.IsFrontendClient && !context.IsNoEnrichment();
        }
    }
}
