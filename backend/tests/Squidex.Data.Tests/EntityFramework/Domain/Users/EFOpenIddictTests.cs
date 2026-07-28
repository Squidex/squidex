// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Squidex.EntityFramework.Domain.Users;

public abstract class EFOpenIddictTests<TContext>(ISqlFixture<TContext> fixture)
    where TContext : DbContext, IDbContextWithDialect
{
    [Fact]
    public async Task Should_allow_openiddict_tokens_without_application()
    {
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();

        var authorization = new OpenIddictEntityFrameworkCoreAuthorization
        {
            Id = Guid.NewGuid().ToString(),
            ApplicationId = null,
            CreationDate = DateTime.UtcNow,
            Status = Statuses.Valid,
            Subject = "admin@squidex.io",
            Type = AuthorizationTypes.Permanent,
        };

        var token = new OpenIddictEntityFrameworkCoreToken
        {
            Id = Guid.NewGuid().ToString(),
            ApplicationId = null,
            Authorization = authorization,
            CreationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddMinutes(5),
            Status = Statuses.Valid,
            Subject = "admin@squidex.io",
            Type = "authorization_code",
        };

        dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization>().Add(authorization);
        dbContext.Set<OpenIddictEntityFrameworkCoreToken>().Add(token);

        await dbContext.SaveChangesAsync();
    }
}
