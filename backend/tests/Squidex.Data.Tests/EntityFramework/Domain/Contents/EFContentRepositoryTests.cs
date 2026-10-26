// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents;

public abstract class EFContentRepositoryTests<TContext, TContentContext>(ISqlContentFixture<TContext, TContentContext> fixture)
    : ContentRepositoryTests
    where TContext : DbContext, IDbContextWithDialect where TContentContext : ContentDbContext
{
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema1");

    protected override Task<IContentRepository> CreateSutAsync()
    {
        var sut =
            new EFContentRepository<TContext, TContentContext>(
                fixture.DbContextFactory,
                fixture.DbContextNamedFactory,
                AppProvider,
                Options.Create(new ContentsOptions()));

        return Task.FromResult<IContentRepository>(sut);
    }

    [Fact]
    public async Task Should_ignore_custom_indexes_without_dedicated_tables()
    {
        var index = new IndexDefinition
        {
            new IndexField("data.field1.iv", SortOrder.Ascending),
        };

        var sut = await CreateSutAsync();

        await sut.CreateIndexAsync(appId.Id, schemaId.Id, index);
        try
        {
            var indexesCreated = await sut.GetIndexesAsync(appId.Id, schemaId.Id);

            // Custom indexes would apply to the contents of all schemas in the shared table.
            Assert.Empty(indexesCreated);
        }
        finally
        {
            await sut.DropIndexAsync(appId.Id, schemaId.Id, index.ToName());
        }
    }
}
