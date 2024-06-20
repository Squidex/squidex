// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class ResolveReferences : IContentEnricherStep
{
    private static readonly ILookup<DomainId, EnrichedContent> EmptyContents = Enumerable.Empty<EnrichedContent>().ToLookup(x => x.Id);
    private readonly Lazy<IContentQueryService> contentQuery;
    private readonly IRequestCache requestCache;

    private IContentQueryService ContentQuery
    {
        get => contentQuery.Value;
    }

    public ResolveReferences(Lazy<IContentQueryService> contentQuery, IRequestCache requestCache)
    {
        this.contentQuery = contentQuery;
        this.requestCache = requestCache;
    }

    public async Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        if (!ShouldEnrich(context))
        {
            return;
        }

        var ids = new HashSet<DomainId>();

        // Group by schema, so we only fetch the schema once.
        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            var (schema, components) = await schemas(group.Key);

            AddReferenceIds(ids, schema, components, group);
        }

        var references = await GetReferencesAsync(context, ids, ct);

        // Group by schema, so we only fetch the schema once.
        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            var (schema, components) = await schemas(group.Key);

            await ResolveReferencesAsync(context, schema, components, group, references, schemas);
        }
    }

    private async Task ResolveReferencesAsync(Context context, Schema schema, ResolvedComponents components,
        IEnumerable<EnrichedContent> contents, ILookup<DomainId, EnrichedContent> references, ProvideSchema schemas)
    {
        HashSet<DomainId>? fieldIds = null;

        var formatted = new Dictionary<EnrichedContent, JsonObject>();

        foreach (var field in schema.ResolvingReferences())
        {
            foreach (var content in contents)
            {
                content.ReferenceData ??= [];

                var fieldReference = content.ReferenceData.GetOrAdd(field.Name, _ => [])!;

                try
                {
                    if (content.Data.TryGetValue(field.Name, out var fieldData) && fieldData != null)
                    {
                        foreach (var (partition, partitionValue) in fieldData)
                        {
                            fieldIds ??= [];
                            fieldIds.Clear();

                            partitionValue.AddReferencedIds(field, fieldIds, components);

                            var referencedContents =
                                fieldIds
                                    .Select(x => references[x])
                                    .SelectMany(x => x)
                                    .ToList();

                            if (referencedContents.Count == 1)
                            {
                                var reference = referencedContents[0];

                                var (referencedSchema, _) = await schemas(reference.SchemaId.Id);

                                requestCache.AddDependency(referencedSchema.UniqueId, referencedSchema.Version);
                                requestCache.AddDependency(reference.UniqueId, reference.Version);

                                var value = formatted.GetOrAdd(reference, x => Format(x, context, referencedSchema));

                                fieldReference.AddLocalized(partition, value);
                            }
                            else if (referencedContents.Count > 1)
                            {
                                var value = CreateFallback(context, referencedContents);

                                fieldReference.AddLocalized(partition, value);
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

    private static JsonObject Format(Content content, Context context, Schema referencedSchema)
    {
        return content.Data.FormatReferences(referencedSchema, context.App.Languages);
    }

    private static JsonObject CreateFallback(Context context, List<EnrichedContent> referencedContents)
    {
        var text = T.Get("contents.listReferences", new { count = referencedContents.Count });

        var value = new JsonObject();

        foreach (var partitionKey in context.App.Languages.AllKeys)
        {
            value.Add(partitionKey, text);
        }

        return value;
    }

    private static void AddReferenceIds(HashSet<DomainId> ids, Schema schema, ResolvedComponents components, IEnumerable<EnrichedContent> contents)
    {
        foreach (var content in contents)
        {
            content.Data.AddReferencedIds(schema.ResolvingReferences(), ids, components);
        }
    }

    private async Task<ILookup<DomainId, EnrichedContent>> GetReferencesAsync(Context context, HashSet<DomainId> ids,
        CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return EmptyContents;
        }

        // Ensure that we reset the fields to not use the field selection from the parent query.
        var queryContext = context.Clone(b => b
            .WithFields(null)
            .WithNoEnrichment(true)
            .WithNoTotal());

        var references = await ContentQuery.QueryAsync(queryContext, Q.Empty.WithIds(ids).WithoutTotal(), ct);

        return references.ToLookup(x => x.Id);
    }

    private static bool ShouldEnrich(Context context)
    {
        return context.IsFrontendClient && !context.NoEnrichment();
    }
}
