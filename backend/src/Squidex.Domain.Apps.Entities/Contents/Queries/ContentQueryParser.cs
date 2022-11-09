// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Squidex.Domain.Apps.Core.GenerateFilters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Queries.OData;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ContentQueryParser
{
    private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(60);
    private readonly IMemoryCache cache;
    private readonly IJsonSerializer serializer;
    private readonly IAppProvider appprovider;
    private readonly ITextIndex textIndex;
    private readonly ContentOptions options;

    public ContentQueryParser(IAppProvider appprovider, ITextIndex textIndex, IOptions<ContentOptions> options,
        IMemoryCache cache, IJsonSerializer serializer)
    {
        this.serializer = serializer;
        this.appprovider = appprovider;
        this.textIndex = textIndex;
        this.cache = cache;
        this.options = options.Value;
    }

    public virtual async Task<Q> ParseAsync(Context context, Q q, ISchemaEntity? schema = null)
    {
        Guard.NotNull(context);
        Guard.NotNull(q);

        using (Telemetry.Activities.StartActivity("ContentQueryParser/ParseAsync"))
        {
            var query = await ParseClrQueryAsync(context, q, schema);

            await TransformFilterAsync(query, context, schema);

            WithSorting(query);
            WithPaging(query, q);

            q = q.WithQuery(query);

            if (context.ShouldSkipTotal())
            {
                q = q.WithoutTotal();
            }
            else if (context.ShouldSkipSlowTotal())
            {
                q = q.WithoutSlowTotal();
            }

            return q;
        }
    }

    private async Task TransformFilterAsync(ClrQuery query, Context context, ISchemaEntity? schema)
    {
        if (query.Filter != null && schema != null)
        {
            query.Filter = await GeoQueryTransformer.TransformAsync(query.Filter, context, schema, textIndex);
        }

        if (!string.IsNullOrWhiteSpace(query.FullText))
        {
            if (schema == null)
            {
                ThrowHelper.InvalidOperationException();
                return;
            }

            var textQuery = new TextQuery(query.FullText, 1000)
            {
                PreferredSchemaId = schema.Id
            };

            var fullTextIds = await textIndex.SearchAsync(context.App, textQuery, context.Scope());
            var fullTextFilter = ClrFilter.Eq("id", "__notfound__");

            if (fullTextIds?.Any() == true)
            {
                fullTextFilter = ClrFilter.In("id", fullTextIds.Select(x => x.ToString()).ToList());
            }

            if (query.Filter != null)
            {
                query.Filter = ClrFilter.And(query.Filter, fullTextFilter);
            }
            else
            {
                query.Filter = fullTextFilter;
            }

            query.FullText = null;
        }
    }

    private async Task<ClrQuery> ParseClrQueryAsync(Context context, Q q, ISchemaEntity? schema)
    {
        var components = ResolvedComponents.Empty;

        if (schema != null)
        {
            components = await appprovider.GetComponentsAsync(schema);
        }

        var query = q.Query;

        if (!string.IsNullOrWhiteSpace(q.QueryAsJson))
        {
            query = ParseJson(context, schema, q.QueryAsJson, components);
        }
        else if (q?.JsonQuery != null)
        {
            query = ParseJson(context, schema, q.JsonQuery, components);
        }
        else if (!string.IsNullOrWhiteSpace(q?.QueryAsOdata))
        {
            query = ParseOData(context, schema, q.QueryAsOdata, components);
        }

        return query;
    }

    private static void WithSorting(ClrQuery query)
    {
        query.Sort ??= new List<SortNode>();

        if (query.Sort.Count == 0)
        {
            query.Sort.Add(new SortNode(new List<string> { "lastModified" }, SortOrder.Descending));
        }

        if (!query.Sort.Any(x => string.Equals(x.Path.ToString(), "id", StringComparison.OrdinalIgnoreCase)))
        {
            query.Sort.Add(new SortNode(new List<string> { "id" }, SortOrder.Ascending));
        }
    }

    private void WithPaging(ClrQuery query, Q q)
    {
        if (query.Take is <= 0 or long.MaxValue)
        {
            if (q.Ids is { Count: > 0 })
            {
                query.Take = q.Ids.Count;
            }
            else
            {
                query.Take = options.DefaultPageSize;
            }
        }
        else if (query.Take > options.MaxResults)
        {
            query.Take = options.MaxResults;
        }
    }

    private ClrQuery ParseJson(Context context, ISchemaEntity? schema, Query<JsonValue> query,
        ResolvedComponents components)
    {
        var queryModel = BuildQueryModel(context, schema, components);

        return queryModel.Convert(query);
    }

    private ClrQuery ParseJson(Context context, ISchemaEntity? schema, string json,
        ResolvedComponents components)
    {
        var queryModel = BuildQueryModel(context, schema, components);

        return queryModel.Parse(json, serializer);
    }

    private ClrQuery ParseOData(Context context, ISchemaEntity? schema, string odata,
        ResolvedComponents components)
    {
        try
        {
            var model = BuildEdmModel(context, schema, components);

            return model.ParseQuery(odata).ToQuery();
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (NotSupportedException)
        {
            throw new ValidationException(T.Get("common.odataNotSupported", new { odata }));
        }
        catch (ODataException ex)
        {
            var message = ex.Message;

            throw new ValidationException(T.Get("common.odataFailure", new { odata, message }), ex);
        }
        catch (Exception)
        {
            throw new ValidationException(T.Get("common.odataNotSupported", new { odata }));
        }
    }

    private QueryModel BuildQueryModel(Context context, ISchemaEntity? schema,
        ResolvedComponents components)
    {
        var cacheKey = BuildJsonCacheKey(context.App, schema, context.IsFrontendClient);

        var result = cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTime;

            return ContentQueryModel.Build(schema?.SchemaDef, context.App.PartitionResolver(), components);
        })!;

        return result;
    }

    private IEdmModel BuildEdmModel(Context context, ISchemaEntity? schema,
        ResolvedComponents components)
    {
        var cacheKey = BuildEmdCacheKey(context.App, schema, context.IsFrontendClient);

        var result = cache.GetOrCreate<IEdmModel>(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTime;

            return BuildQueryModel(context, schema, components).ConvertToEdm("Contents", schema?.SchemaDef.Name ?? "Generic");
        })!;

        return result;
    }

    private static string BuildEmdCacheKey(IAppEntity app, ISchemaEntity? schema, bool withHidden)
    {
        if (schema == null)
        {
            return $"EDM/__generic";
        }

        return $"EDM/{app.Version}/{schema.Id}_{schema.Version}/{withHidden}";
    }

    private static string BuildJsonCacheKey(IAppEntity app, ISchemaEntity? schema, bool withHidden)
    {
        if (schema == null)
        {
            return $"JSON/__generic";
        }

        return $"JSON/{app.Version}/{schema.Id}_{schema.Version}/{withHidden}";
    }
}
