// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.EntityFramework.Domain.Contents.Text;

public abstract class EFTextIndexTests<TContext>(ISqlFixture<TContext> fixture) : TextIndexerTests
    where TContext : DbContext, IDbContextWithDialect
{
    public override bool SupportsQuerySyntax => false;

    public override bool SupportsGeo => true;

    public override async Task<ITextIndex> CreateSutAsync()
    {
        var sut = new EFTextIndex<TContext>(fixture.DbContextFactory);

        await sut.InitializeAsync(default);
        return sut;
    }
}
