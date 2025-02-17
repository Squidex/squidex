// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class EFTextIndex<TContext>(IDbContextFactory<TContext> dbContextFactory, SqlDialect dialect)
    : ITextIndex, IInitializable, IDeleter where TContext : DbContext
{
    private record struct SearchOperation
    {
        required public App App { get; init; }

        required public List<(DomainId Id, double Score)> Results { get; init; }

        required public string SearchTerms { get; init; }

        required public int Take { get; set; }

        required public SearchScope SearchScope { get; init; }
    }

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Database.CreateTextIndexAsync(dialect, "IDX_Text", "Texts", "Texts", ct);
        await dbContext.Database.CreateGeoIndexAsync(dialect, "IDX_Geo", "Geos", "GeoObject", ct);
    }

    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFTextIndexEntity>().Where(x => x.AppId == app.Id)
            .ExecuteDeleteAsync(ct);

        await dbContext.Set<EFGeoEntity>().Where(x => x.AppId == app.Id)
            .ExecuteDeleteAsync(ct);
    }

    async Task IDeleter.DeleteSchemaAsync(App app, Schema schema,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFTextIndexEntity>().Where(x => x.AppId == app.Id && x.SchemaId == schema.Id)
            .ExecuteDeleteAsync(ct);

        await dbContext.Set<EFGeoEntity>().Where(x => x.AppId == app.Id && x.SchemaId == schema.Id)
            .ExecuteDeleteAsync(ct);
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFTextIndexEntity>()
            .ExecuteDeleteAsync(ct);

        await dbContext.Set<EFGeoEntity>()
            .ExecuteDeleteAsync(ct);
    }

    public async Task<List<DomainId>?> SearchAsync(App app, GeoQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(query);

        await using var dbContext = await CreateDbContextAsync(ct);

        var point = new Point(query.Latitude, query.Longitude) { SRID = 4326 };
        var ids =
            await dbContext.Set<EFGeoEntity>()
                .Where(x => x.AppId == app.Id)
                .Where(x => x.SchemaId == query.SchemaId)
                .Where(x => x.GeoField == query.Field)
                .Where(x => x.GeoObject.Distance(point) < query.Radius)
                .WhereScope(scope)
                .Select(x => x.ContentId)
                .ToListAsync(ct);

        return ids;
    }

    public async Task<List<DomainId>?> SearchAsync(App app, TextQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        Guard.NotNull(app);
        Guard.NotNull(query);

        if (string.IsNullOrWhiteSpace(query.Text))
        {
            return null;
        }

        // Use a custom tokenizer to leverage stop words from multiple languages.
        var search = new SearchOperation
        {
            App = app,
            SearchTerms = Tokenizer.Query(query.Text),
            SearchScope = scope,
            Results = [],
            Take = query.Take,
        };

        await using var dbContext = await CreateDbContextAsync(ct);

        if (query.RequiredSchemaIds?.Count > 0)
        {
            await SearchBySchemaAsync(dbContext, search, query.RequiredSchemaIds, 1, ct);
        }
        else if (query.PreferredSchemaId == null)
        {
            await SearchByAppAsync(dbContext, search, 1, ct);
        }
        else
        {
            // We cannot write queries that prefer results from the same schema, therefore make two queries.
            search.Take /= 2;

            // Increasing the scoring of the results from the schema by 10 percent.
            await SearchBySchemaAsync(dbContext, search, Enumerable.Repeat(query.PreferredSchemaId.Value, 1), 1.1, ct);
            await SearchByAppAsync(dbContext, search, 1, ct);
        }

        return search.Results.OrderByDescending(x => x.Score).Select(x => x.Id).Distinct().ToList();
    }

    private Task SearchBySchemaAsync(TContext context, SearchOperation search, IEnumerable<DomainId> schemaIds, double factor,
        CancellationToken ct = default)
    {
        var queryBuilder =
            new SqlQueryBuilder(dialect, "Texts")
                .Where(ClrFilter.Eq("AppId", search.App.Id))
                .Where(ClrFilter.In("SchemaId", schemaIds.ToList()))
                .WhereMatch("Texts", search.SearchTerms)
                .WhereScope(search.SearchScope);

        return SearchAsync(context, search, queryBuilder, factor, ct);
    }

    private Task SearchByAppAsync(TContext context, SearchOperation search, double factor,
        CancellationToken ct = default)
    {
        var queryBuilder =
            new SqlQueryBuilder(dialect, "Texts")
                .Where(ClrFilter.Eq("AppId", search.App.Id))
                .WhereMatch("Texts", search.SearchTerms)
                .WhereScope(search.SearchScope);

        return SearchAsync(context, search, queryBuilder, factor, ct);
    }

    private static async Task SearchAsync(TContext context, SearchOperation search, SqlQueryBuilder queryBuilder, double factor,
        CancellationToken ct)
    {
        var (sql, parameters) = queryBuilder.Compile();

        var ids =
            await context.Set<EFTextIndexEntity>().FromSqlRaw(sql, parameters)
                .Select(x => x.ContentId)
                .ToListAsync(ct);

        search.Results.AddRange(ids.Select(x => (x, 1 * factor)));
    }

    public async Task ExecuteAsync(IndexCommand[] commands,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var insertsText = new List<EFTextIndexEntity>();
        var insertsGeo = new List<EFGeoEntity>();

        foreach (var batch in commands.Batch(1000))
        {
            foreach (var command in batch)
            {
                var (appId, contentId) = command.UniqueContentId;
                var id = $"{appId}_{contentId}_{command.Stage}";
                switch (command)
                {
                    case UpsertIndexEntry upsert:
                        if (upsert.Texts != null)
                        {
                            insertsText.Add(new EFTextIndexEntity
                            {
                                Id = id,
                                AppId = appId,
                                ContentId = contentId,
                                SchemaId = upsert.SchemaId.Id,
                                ServeAll = upsert.ServeAll,
                                ServePublished = upsert.ServePublished,
                                Stage = upsert.Stage,
                                Texts = Tokenizer.Terms(upsert.Texts),
                            });
                        }

                        foreach (var (field, obj) in upsert.GeoObjects.OrEmpty())
                        {
                            obj.SRID = 4326;

                            insertsGeo.Add(new EFGeoEntity
                            {
                                Id = id,
                                AppId = appId,
                                ContentId = contentId,
                                GeoField = field,
                                GeoObject = obj,
                                SchemaId = upsert.SchemaId.Id,
                                ServeAll = upsert.ServeAll,
                                ServePublished = upsert.ServePublished,
                                Stage = upsert.Stage,
                            });
                        }

                        break;
                    case DeleteIndexEntry:
                        await dbContext.Set<EFTextIndexEntity>()
                            .Where(x => x.Id == id)
                            .ExecuteDeleteAsync(ct);

                        await dbContext.Set<EFGeoEntity>()
                            .Where(x => x.Id == id)
                            .ExecuteDeleteAsync(ct);
                        break;
                    case UpdateIndexEntry update:
                        await dbContext.Set<EFTextIndexEntity>()
                            .Where(x => x.Id == id)
                            .ExecuteUpdateAsync(u => u
                                .SetProperty(x => x.ServeAll, update.ServeAll)
                                .SetProperty(x => x.ServePublished, update.ServePublished),
                                ct);

                        await dbContext.Set<EFGeoEntity>()
                            .Where(x => x.Id == id)
                            .ExecuteUpdateAsync(u => u
                                .SetProperty(x => x.ServeAll, update.ServeAll)
                                .SetProperty(x => x.ServePublished, update.ServePublished),
                                ct);
                        break;
                }
            }
        }

        await dbContext.BulkInsertOrUpdateAsync(insertsText, cancellationToken: ct);
        await dbContext.BulkInsertOrUpdateAsync(insertsGeo, cancellationToken: ct);
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
