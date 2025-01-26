// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Schemas;

[Trait("Category", "TestContainer")]
public class MongoSchemaRepositoryTests(MongoFixture fixture) : SchemaRepositoryTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoSchemaRepository sut = new MongoSchemaRepository(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<ISchemaRepository> CreateSutAsync()
    {
        return Task.FromResult<ISchemaRepository>(sut);
    }
}
