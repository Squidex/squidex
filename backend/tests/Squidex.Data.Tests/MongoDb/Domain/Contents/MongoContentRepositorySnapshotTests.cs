// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Infrastructure.States;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Contents;

[Trait("Category", "TestContainer")]
[Collection("Mongo")]
public class MongoContentRepositorySnapshotTests(MongoFixture fixture) : ContentSnapshotStoreTests
{
    protected override bool CheckConsistencyOnWrite => false;

    protected override async Task<ISnapshotStore<WriteContent>> CreateSutAsync()
    {
        var sut =
            new MongoContentRepository(
                fixture.Database,
                Context.AppProvider,
                string.Empty,
                Options.Create(new ContentsOptions()),
                A.Fake<ILogger<MongoContentRepository>>());

        await sut.InitializeAsync(default);
        return sut;
    }
}
