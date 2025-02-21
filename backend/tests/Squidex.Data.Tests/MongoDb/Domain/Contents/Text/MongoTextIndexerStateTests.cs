// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Contents.Text;

[Trait("Category", "TestContainer")]
[Collection("Mongo")]
public class MongoTextIndexerStateTests(MongoFixture fixture) : TextIndexerStateTests
{
    protected override async Task<ITextIndexerState> CreateSutAsync(IContentRepository contentRepository)
    {
        var sut = new MongoTextIndexerState(fixture.Database, contentRepository);

        await sut.InitializeAsync(default);
        return sut;
    }
}
