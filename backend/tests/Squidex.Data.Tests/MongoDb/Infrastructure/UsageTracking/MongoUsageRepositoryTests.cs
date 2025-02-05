// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.UsageTracking;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Infrastructure.UsageTracking;

[Trait("Category", "TestContainer")]
[Collection("Mongo")]
public class MongoUsageRepositoryTests(MongoFixture fixture) : UsageRepositoryTests
{
    protected override async Task<IUsageRepository> CreateSutAsync()
    {
        var sut = new MongoUsageRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
