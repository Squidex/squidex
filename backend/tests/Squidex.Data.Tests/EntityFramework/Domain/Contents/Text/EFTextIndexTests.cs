// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.EntityFramework.TestHelpers;

namespace Squidex.EntityFramework.Domain.Contents.Text;

public abstract class EFTextIndexTests<TContext>(ISqlFixture<TContext> fixture) : TextIndexerTestsBase where TContext : DbContext
{
    public override bool SupportsQuerySyntax => false;

    public override async Task<ITextIndex> CreateSutAsync()
    {
        var sut = new EFTextIndex<TContext>(fixture.DbContextFactory, fixture.Dialect);

        await sut.InitializeAsync(default);
        return sut;
    }
}
