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

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class ResolveReferences : IContentEnricherStep
    {
        private static readonly ILookup<Guid, IEnrichedContentEntity> EmptyContents = Enumerable.Empty<IEnrichedContentEntity>().ToLookup(x => x.Id);
        private readonly Lazy<IContentQueryService> contentQuery;

        private IContentQueryService ContentQuery
        {
            get { return contentQuery.Value; }
        }

        public ResolveReferences(Lazy<IContentQueryService> contentQuery)
        {
            Guard.NotNull(contentQuery);

            this.contentQuery = contentQuery;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
            if (ShouldEnrich(context))
            {
                var ids = new HashSet<Guid>();

                foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
                {
                    var schema = await schemas(group.Key);

                    AddReferenceIds(ids, schema, group);
                }

                var references = await GetReferencesAsync(context, ids);

                foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
                {
                    var schema = await schemas(group.Key);

                    await ResolveReferencesAsync(context, schema, group, references, schemas);
                }
            }
        }

        private async Task ResolveReferencesAsync(Context context, ISchemaEntity schema, IEnumerable<ContentEntity> contents, ILookup<Guid, IEnrichedContentEntity> references, ProvideSchema schemas)
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
                            foreach (var (partition, partitionValue) in fieldData)
                            {
                                var referencedContents =
                                    field.GetReferencedIds(partitionValue)
                                        .Select(x => references[x])
                                        .SelectMany(x => x)
                                        .ToList();

                                if (referencedContents.Count == 1)
                                {
                                    var reference = referencedContents[0];

                                    var referencedSchema = await schemas(reference.SchemaId.Id);

                                    content.CacheDependencies ??= new HashSet<object?>();

                                    content.CacheDependencies.Add(referencedSchema.Id);
                                    content.CacheDependencies.Add(referencedSchema.Version);
                                    content.CacheDependencies.Add(reference.Id);
                                    content.CacheDependencies.Add(reference.Version);

                                    var value = formatted.GetOrAdd(reference, x => Format(x, context, referencedSchema));

                                    fieldReference.AddJsonValue(partition, value);
                                }
                                else if (referencedContents.Count > 1)
                                {
                                    var value = CreateFallback(context, referencedContents);

                                    fieldReference.AddJsonValue(partition, value);
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

        private static JsonObject Format(IContentEntity content, Context context, ISchemaEntity referencedSchema)
        {
            return content.DataDraft.FormatReferences(referencedSchema.SchemaDef, context.App.LanguagesConfig);
        }

        private static JsonObject CreateFallback(Context context, List<IEnrichedContentEntity> referencedContents)
        {
            var text = $"{referencedContents.Count} Reference(s)";

            var value = JsonValue.Object();

            foreach (var partitionKey in context.App.LanguagesConfig.AllKeys)
            {
                value.Add(partitionKey, text);
            }

            return value;
        }

        private void AddReferenceIds(HashSet<Guid> ids, ISchemaEntity schema, IEnumerable<ContentEntity> contents)
        {
            foreach (var content in contents)
            {
                content.DataDraft.AddReferencedIds(schema.SchemaDef.ResolvingReferences(), ids);
            }
        }

        private async Task<ILookup<Guid, IEnrichedContentEntity>> GetReferencesAsync(Context context, HashSet<Guid> ids)
        {
            if (ids.Count == 0)
            {
                return EmptyContents;
            }

            var references = await ContentQuery.QueryAsync(context.Clone().WithoutContentEnrichment(true), ids.ToList());

            return references.ToLookup(x => x.Id);
        }

        private static bool ShouldEnrich(Context context)
        {
            return context.IsFrontendClient && context.ShouldEnrichContent();
        }
    }
}
