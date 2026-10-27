// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;
using Squidex.Migrations;
using Squidex.Providers.MySql;
using Squidex.Providers.Postgres;
using Squidex.Providers.SqlServer;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents;

[ReuseLabel("dedicated")]
public abstract class EFContentRepositoryDedicatedTests<TContext, TContentContext>(ISqlContentFixture<TContext, TContentContext> fixture)
    : ContentRepositoryTests
    where TContext : DbContext, IDbContextWithDialect where TContentContext : ContentDbContext
{
    private readonly IOptions<ContentsOptions> options = Options.Create(new ContentsOptions { OptimizeForSelfHosting = true });
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId1 = NamedId.Of(DomainId.NewGuid(), "my-schema1");
    private readonly NamedId<DomainId> schemaId2 = NamedId.Of(DomainId.NewGuid(), "my-schema2");

    protected override bool SupportsCustomIndexes => true;

    protected override Task<IContentRepository> CreateSutAsync()
    {
        var sut =
            new EFContentRepository<TContext, TContentContext>(
                fixture.DbContextFactory,
                fixture.DbContextNamedFactory,
                AppProvider,
                options);

        return Task.FromResult<IContentRepository>(sut);
    }

    [Fact]
    public async Task Should_use_own_model_for_each_prefix()
    {
        await using var context1 = await fixture.DbContextNamedFactory.CreateDbContextAsync("__cTest1_");
        await using var context2 = await fixture.DbContextNamedFactory.CreateDbContextAsync("__cTest2_");

        var table1 = context1.Model.FindEntityType(typeof(EFContentCompleteEntity))!.GetTableName();
        var table2 = context2.Model.FindEntityType(typeof(EFContentCompleteEntity))!.GetTableName();

        Assert.Equal("__cTest1_ContentsAll", table1);
        Assert.Equal("__cTest2_ContentsAll", table2);
    }

    [Fact]
    public async Task Should_move_contents_into_correct_dedicated_tables()
    {
        var content1 = CreateWriteContent() with { Id = DomainId.NewGuid(), AppId = appId, SchemaId = schemaId1 };
        var content2 = CreateWriteContent() with { Id = DomainId.NewGuid(), AppId = appId, SchemaId = schemaId2 };

        var sut = await CreateSutAsync();

        await ((ISnapshotStore<WriteContent>)sut).WriteManyAsync(
        [
            new SnapshotWriteJob<WriteContent>(content1.UniqueId, content1, 0),
            new SnapshotWriteJob<WriteContent>(content2.UniqueId, content2, 0),
        ]);

        // Simulate the old behavior, where all contents were written to the first dedicated table.
        var dynamicTables = new DynamicTables<TContext, TContentContext>(fixture.DbContextFactory, fixture.DbContextNamedFactory);

        await using (var context1 = await dynamicTables.CreateDbContextAsync(appId.Id, schemaId1.Id, default))
        {
            await using var context2 = await dynamicTables.CreateDbContextAsync(appId.Id, schemaId2.Id, default);

            var misplaced = await context2.Set<EFContentCompleteEntity>().ToListAsync();

            await context2.Set<EFContentCompleteEntity>().ExecuteDeleteAsync();
            await context2.Set<EFContentPublishedEntity>().ExecuteDeleteAsync();

            context1.AddRange(misplaced);
            await context1.SaveChangesAsync();
        }

        var migration =
            new RebuildDedicatedContentTables<TContext, TContentContext>(
                fixture.DbContextFactory,
                fixture.DbContextNamedFactory,
                options,
                NullLogger<RebuildDedicatedContentTables<TContext, TContentContext>>.Instance);

        await migration.UpdateAsync(default);

        var schema1 = Schema with { AppId = appId, Id = schemaId1.Id, Name = schemaId1.Name };
        var schema2 = Schema with { AppId = appId, Id = schemaId2.Id, Name = schemaId2.Name };

        var app = App with { Id = appId.Id, Name = appId.Name };

        var result1All = await sut.QueryAsync(app, schema1, Q.Empty, SearchScope.All);
        var result2All = await sut.QueryAsync(app, schema2, Q.Empty, SearchScope.All);
        var result2Published = await sut.QueryAsync(app, schema2, Q.Empty, SearchScope.Published);

        Assert.Equal([content1.Id], result1All.Select(x => x.Id).ToArray());
        Assert.Equal([content2.Id], result2All.Select(x => x.Id).ToArray());
        Assert.Equal([content2.Id], result2Published.Select(x => x.Id).ToArray());
    }

    [Theory]
    [InlineData("data.field1.iv", SortOrder.Ascending)]
    [InlineData("data.field1.iv", SortOrder.Descending)]
    [InlineData("translationStatus.en", SortOrder.Ascending)]
    [InlineData("lastModified", SortOrder.Descending)]
    public async Task Should_sort_with_custom_index(string field, SortOrder order)
    {
        var index = new IndexDefinition
        {
            new IndexField(field, order),
        };

        var sut = await CreateSutAsync();

        await sut.CreateIndexAsync(appId.Id, schemaId1.Id, index);
        try
        {
            var dynamicTables = new DynamicTables<TContext, TContentContext>(fixture.DbContextFactory, fixture.DbContextNamedFactory);

            await using var dbContext = await dynamicTables.CreateDbContextAsync(appId.Id, schemaId1.Id, default);

            var table = GetTableName<EFContentCompleteEntity>(dbContext);

            var indexesCustom = await GetCustomIndexNamesAsync(dbContext, table);
            var indexName = Assert.Single(indexesCustom);

            // Use the same query builder as the repository, otherwise the ORDER BY clause could be different.
            var sqlQuery =
                new ContentSqlQueryBuilder(dbContext.Dialect, table)
                    .Order(field, order)
                    .Limit(20)
                    .CompileQuery();

            var (plan, sortMarker) = await ExplainAsync(dbContext, table, indexName, sqlQuery);

            Assert.Contains(indexName, plan, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(sortMarker, plan, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await sut.DropIndexAsync(appId.Id, schemaId1.Id, index.ToName());
        }
    }

    [Fact]
    public async Task Should_create_and_drop_custom_index_in_all_and_published_table()
    {
        var sut = await CreateSutAsync();

        await sut.CreateIndexAsync(appId.Id, schemaId1.Id, [new IndexField("data.field1.iv", SortOrder.Ascending)]);

        var dynamicTables = new DynamicTables<TContext, TContentContext>(fixture.DbContextFactory, fixture.DbContextNamedFactory);

        await using var dbContext = await dynamicTables.CreateDbContextAsync(appId.Id, schemaId1.Id, default);

        var tableAll = GetTableName<EFContentCompleteEntity>(dbContext);
        var tablePublished = GetTableName<EFContentPublishedEntity>(dbContext);

        var createdAll = await GetCustomIndexNamesAsync(dbContext, tableAll);
        var createdPublished = await GetCustomIndexNamesAsync(dbContext, tablePublished);

        Assert.Single(createdAll);
        Assert.Single(createdPublished);

        await sut.DropIndexAsync(appId.Id, schemaId1.Id, "data.field1.iv_asc");

        var droppedAll = await GetCustomIndexNamesAsync(dbContext, tableAll);
        var droppedPublished = await GetCustomIndexNamesAsync(dbContext, tablePublished);

        Assert.Empty(droppedAll);
        Assert.Empty(droppedPublished);
    }

    private static string GetTableName<T>(DbContext dbContext)
    {
        return dbContext.Model.FindEntityType(typeof(T))!.GetTableName()!;
    }

    private static async Task<List<string>> GetCustomIndexNamesAsync(TContentContext dbContext, string table)
    {
        var indexes = await dbContext.Dialect.QueryIndexesAsync(dbContext, table, default);

        return indexes.Select(x => x.Name).Where(x => x.StartsWith($"IX_{table}_cx_", StringComparison.Ordinal)).ToList();
    }

    private static async Task<(string Plan, string SortMarker)> ExplainAsync(TContentContext dbContext, string table, string indexName, string sqlQuery)
    {
        // The tables are almost empty, therefore the planners are pushed to the index. They can still sort, if the index does not match.
        switch (dbContext.Dialect)
        {
            case PostgresDialect:
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();
                    try
                    {
                        await dbContext.Database.ExecuteSqlRawAsync("SET LOCAL enable_sort = off;");

                        var plan = await ReadAllAsync(dbContext, $"EXPLAIN {sqlQuery}");

                        return (plan, "Sort");
                    }
                    finally
                    {
                        await transaction.RollbackAsync();
                    }
                }

            case MySqlDialect:
                {
                    var sqlHinted = sqlQuery.Replace($"FROM `{table}`", $"FROM `{table}` FORCE INDEX (`{indexName}`)", StringComparison.Ordinal);

                    var plan = await ReadAllAsync(dbContext, $"EXPLAIN {sqlHinted}");

                    return (plan, "filesort");
                }

            case SqlServerDialect:
                {
                    var sqlHinted = sqlQuery.Replace($"FROM [{table}]", $"FROM [{table}] WITH (INDEX([{indexName}]))", StringComparison.Ordinal);

                    await dbContext.Database.OpenConnectionAsync();
                    try
                    {
                        await ReadAllAsync(dbContext, "SET SHOWPLAN_XML ON;");
                        try
                        {
                            var plan = await ReadAllAsync(dbContext, sqlHinted);

                            return (plan, "PhysicalOp=\"Sort\"");
                        }
                        finally
                        {
                            await ReadAllAsync(dbContext, "SET SHOWPLAN_XML OFF;");
                        }
                    }
                    finally
                    {
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }

            default:
                throw new NotSupportedException();
        }
    }

    private static async Task<string> ReadAllAsync(DbContext dbContext, string sql)
    {
        await dbContext.Database.OpenConnectionAsync();
        try
        {
            await using var command = dbContext.Database.GetDbConnection().CreateCommand();

            command.CommandText = sql;
            command.Transaction = dbContext.Database.CurrentTransaction?.GetDbTransaction();

            await using var reader = await command.ExecuteReaderAsync();

            var result = new StringBuilder();
            do
            {
                while (await reader.ReadAsync())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        result.AppendLine(reader.GetValue(i)?.ToString());
                    }
                }
            }
            while (await reader.NextResultAsync());

            return result.ToString();
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }
}
