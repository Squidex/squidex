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
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateFilters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Text;
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
    private readonly IAppProvider appProvider;
    private readonly ITextIndex textIndex;
    private readonly ContentOptions options;

    public ContentQueryParser(IAppProvider appprovider, ITextIndex textIndex, IOptions<ContentOptions> options,
        IMemoryCache cache, IJsonSerializer serializer)
    {
        this.serializer = serializer;
        this.appProvider = appprovider;
        this.textIndex = textIndex;
        this.cache = cache;
        this.options = options.Value;
    }

    public virtual async Task<Q> ParseAsync(Context context, Q q, Schema? schema = null,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);
        Guard.NotNull(q);

        using (Telemetry.Activities.StartActivity("ContentQueryParser/ParseAsync"))
        {
            var query = await ParseClrQueryAsync(context, q, schema, ct);

            await TransformFilterAsync(query, context, schema, ct);

            WithSorting(query);
            WithPaging(query, q);

            q = q.WithQuery(query);
            q = q.WithFields(context.Fields());

            if (context.NoTotal())
            {
                q = q.WithoutTotal();
            }
            else if (context.NoSlowTotal())
            {
                q = q.WithoutSlowTotal();
            }

            return q;
        }
    }

    private async Task TransformFilterAsync(ClrQuery query, Context context, Schema? schema,
        CancellationToken ct)
    {
        if (query.Filter != null && schema != null)
        {
            query.Filter = await GeoQueryTransformer.TransformAsync(query.Filter, context, schema, textIndex, ct);
        }

        if (string.IsNullOrWhiteSpace(query.FullText))
        {
            return;
        }

        if (schema == null)
        {
            ThrowHelper.InvalidOperationException();
            return;
        }

        var textQuery = new TextQuery(query.FullText, 1000)
        {
            PreferredSchemaId = schema.Id
        };

        var fullTextIds = await textIndex.SearchAsync(context.App, textQuery, context.Scope(), ct);
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

    private async Task<ClrQuery> ParseClrQueryAsync(Context context, Q q, Schema? schema,
        CancellationToken ct)
    {
        var components = ResolvedComponents.Empty;

        if (schema != null)
        {
            components = await appProvider.GetComponentsAsync(schema, ct);
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
        query.Sort ??= [];

        if (query.Sort.Count == 0)
        {
            query.Sort.Add(new SortNode("lastModified", SortOrder.Descending));
        }

        if (!query.Sort.Exists(x => x.Path.Equals("id")))
        {
            query.Sort.Add(new SortNode("id", SortOrder.Ascending));
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

    private ClrQuery ParseJson(Context context, Schema? schema, Query<JsonValue> query,
        ResolvedComponents components)
    {
        var queryModel = BuildQueryModel(context, schema, components);

        return queryModel.Convert(query);
    }

    private ClrQuery ParseJson(Context context, Schema? schema, string json,
        ResolvedComponents components)
    {
        var queryModel = BuildQueryModel(context, schema, components);

        return queryModel.Parse(json, serializer);
    }

    private ClrQuery ParseOData(Context context, Schema? schema, string odata,
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

    private QueryModel BuildQueryModel(Context context, Schema? schema,
        ResolvedComponents components)
    {
        var cacheKey = BuildJsonCacheKey(context.App, schema, context.IsFrontendClient);

        var result = cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTime;

            return ContentQueryModel.Build(schema, context.App.PartitionResolver(), components);
        })!;

        return result;
    }

    private IEdmModel BuildEdmModel(Context context, Schema? schema,
        ResolvedComponents components)
    {
        var cacheKey = BuildEmdCacheKey(context.App, schema, context.IsFrontendClient);

        var result = cache.GetOrCreate<IEdmModel>(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTime;

            return BuildQueryModel(context, schema, components).ConvertToEdm("Contents", schema?.Name ?? "Generic");
        })!;

        return result;
    }

    private static string BuildEmdCacheKey(App app, Schema? schema, bool withHidden)
    {
        if (schema == null)
        {
            return $"EDM/__generic";
        }

        return $"EDM/{app.Version}/{schema.Id}_{schema.Version}/{withHidden}";
    }

    private static string BuildJsonCacheKey(App app, Schema? schema, bool withHidden)
    {
        if (schema == null)
        {
            return $"JSON/__generic";
        }

        return $"JSON/{app.Version}/{schema.Id}_{schema.Version}/{withHidden}";
    }
}
