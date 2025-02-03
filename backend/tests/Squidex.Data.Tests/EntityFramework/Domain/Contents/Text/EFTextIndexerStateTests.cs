﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents.Text;

[Trait("Category", "TestContainer")]
[Collection("Postgres")]
public sealed class EFTextIndexerStateTests(PostgresFixture fixture) : TextIndexerStateTests
{
    protected override Task<ITextIndexerState> CreateSutAsync(IContentRepository contentRepository)
    {
        var sut = new EFTextIndexerState<TestDbContextPostgres>(fixture.DbContextFactory, fixture.Dialect, contentRepository);

        return Task.FromResult<ITextIndexerState>(sut);
    }
}
