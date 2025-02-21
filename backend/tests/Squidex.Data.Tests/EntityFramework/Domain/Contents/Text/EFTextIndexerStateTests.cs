// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents.Text;

public abstract class EFTextIndexerStateTests<TContext>(ISqlFixture<TContext> fixture) : TextIndexerStateTests where TContext : DbContext
{
    protected override Task<ITextIndexerState> CreateSutAsync(IContentRepository contentRepository)
    {
        var sut = new EFTextIndexerState<TContext>(fixture.DbContextFactory, fixture.Dialect, contentRepository);

        return Task.FromResult<ITextIndexerState>(sut);
    }
}
