﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Assets;

[Trait("Category", "TestContainer")]
public class MongoAssetRepositoryTests(MongoFixture fixture) : AssetRepositoryTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoAssetRepository sut = new MongoAssetRepository(fixture.Database, A.Fake<ILogger<MongoAssetRepository>>(), string.Empty);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<IAssetRepository> CreateSutAsync()
    {
        return Task.FromResult<IAssetRepository>(sut);
    }
}
