// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Log;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Infrastructure.Log;

public class MongoRequestLogRepositoryTests(MongoFixture fixture) : RequestLogRepositoryTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoRequestLogRepository sut =
        new MongoRequestLogRepository(fixture.Database,
            Options.Create(new RequestLogStoreOptions()));

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<IRequestLogRepository> CreateSutAsync()
    {
        return Task.FromResult<IRequestLogRepository>(sut);
    }
}
