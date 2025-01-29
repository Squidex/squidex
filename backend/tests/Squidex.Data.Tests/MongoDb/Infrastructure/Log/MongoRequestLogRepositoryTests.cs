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

public class MongoRequestLogRepositoryTests(MongoFixture fixture) : RequestLogRepositoryTests, IClassFixture<MongoFixture>
{
    protected override async Task<IRequestLogRepository> CreateSutAsync()
    {
        var sut =
            new MongoRequestLogRepository(fixture.Database,
                Options.Create(new RequestLogStoreOptions()));

        await sut.InitializeAsync(default);
        return sut;
    }
}
