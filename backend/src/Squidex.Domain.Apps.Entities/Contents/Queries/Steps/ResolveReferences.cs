// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class ResolveReferences : IContentEnricherStep
{
    private static readonly ILookup<DomainId, IEnrichedContentEntity> EmptyContents = Enumerable.Empty<IEnrichedContentEntity>().ToLookup(x => x.Id);
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

    public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        if (!ShouldEnrich(context))
        {
            return;
        }

        var ids = new HashSet<DomainId>();

        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            var (schema, components) = await schemas(group.Key);

            AddReferenceIds(ids, schema, components, group);
        }

        var references = await GetReferencesAsync(context, ids, ct);

        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            var (schema, components) = await schemas(group.Key);

            await ResolveReferencesAsync(context, schema, components, group, references, schemas);
        }
    }

    private async Task ResolveReferencesAsync(Context context, ISchemaEntity schema, ResolvedComponents components,
        IEnumerable<ContentEntity> contents, ILookup<DomainId, IEnrichedContentEntity> references, ProvideSchema schemas)
    {
        HashSet<DomainId>? fieldIds = null;

        var formatted = new Dictionary<IContentEntity, JsonObject>();

        foreach (var field in schema.SchemaDef.ResolvingReferences())
        {
            foreach (var content in contents)
            {
                content.ReferenceData ??= new ContentData();

                var fieldReference = content.ReferenceData.GetOrAdd(field.Name, _ => new ContentFieldData())!;

                try
                {
                    if (content.Data.TryGetValue(field.Name, out var fieldData) && fieldData != null)
                    {
                        foreach (var (partition, partitionValue) in fieldData)
                        {
                            fieldIds ??= new HashSet<DomainId>();
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

    private static JsonObject Format(IContentEntity content, Context context, ISchemaEntity referencedSchema)
    {
        return content.Data.FormatReferences(referencedSchema.SchemaDef, context.App.Languages);
    }

    private static JsonObject CreateFallback(Context context, List<IEnrichedContentEntity> referencedContents)
    {
        var text = T.Get("contents.listReferences", new { count = referencedContents.Count });

        var value = new JsonObject();

        foreach (var partitionKey in context.App.Languages.AllKeys)
        {
            value.Add(partitionKey, text);
        }

        return value;
    }

    private static void AddReferenceIds(HashSet<DomainId> ids, ISchemaEntity schema, ResolvedComponents components, IEnumerable<ContentEntity> contents)
    {
        foreach (var content in contents)
        {
            content.Data.AddReferencedIds(schema.SchemaDef.ResolvingReferences(), ids, components);
        }
    }

    private async Task<ILookup<DomainId, IEnrichedContentEntity>> GetReferencesAsync(Context context, HashSet<DomainId> ids,
        CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return EmptyContents;
        }

        var queryContext = context.Clone(b => b
            .WithoutContentEnrichment(true)
            .WithoutTotal());

        var references = await ContentQuery.QueryAsync(queryContext, Q.Empty.WithIds(ids).WithoutTotal(), ct);

        return references.ToLookup(x => x.Id);
    }

    private static bool ShouldEnrich(Context context)
    {
        return context.IsFrontendClient && !context.ShouldSkipContentEnrichment();
    }
}
