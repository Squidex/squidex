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
[Collection("Mongo")]
public class MongoSchemaRepositoryTests(MongoFixture fixture) : SchemaRepositoryTests
{
    protected override async Task<ISchemaRepository> CreateSutAsync()
    {
        var sut = new MongoSchemaRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
