// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Contents;

[Trait("Category", "TestContainer")]
public class MongoContentRepositoryTests(MongoFixture fixture) : ContentRepositoryTests, IClassFixture<MongoFixture>
{
    protected override async Task<IContentRepository> CreateSutAsync()
    {
        var sut =
            new MongoContentRepository(
                fixture.Database,
                AppProvider,
                string.Empty,
                Options.Create(new ContentsOptions()),
                A.Fake<ILogger<MongoContentRepository>>());

        await sut.InitializeAsync(default);
        return sut;
    }
}
