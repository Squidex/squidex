// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Migrations;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents;

[ReuseLabel("dedicated")]
public abstract class EFContentRepositoryDedicatedTests<TContext, TContentContext>(ISqlContentFixture<TContext, TContentContext> fixture)
    : ContentRepositoryTests
    where TContext : DbContext, IDbContextWithDialect where TContentContext : ContentDbContext
{
    private readonly IOptions<ContentsOptions> options = Options.Create(new ContentsOptions { OptimizeForSelfHosting = true });

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
        var appId = NamedId.Of(DomainId.NewGuid(), "my-app");

        var schemaId1 = NamedId.Of(DomainId.NewGuid(), "my-schema1");
        var schemaId2 = NamedId.Of(DomainId.NewGuid(), "my-schema2");

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
}
