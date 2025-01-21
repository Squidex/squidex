// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;

namespace Squidex.Data.EntityFramework;

public class AppDbContext(DbContextOptions options, IJsonSerializer jsonSerializer) : IdentityDbContext(options)
{
    public DbSet<EFAppEntity> Apps { get; set; }

    public DbSet<EFSchemaEntity> Schemas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.AddChatStore();
        builder.AddEventStore();
        builder.AddMessagingDataStore();
        builder.AddMessagingTransport();
        builder.UseOpenIddict();

        builder.AddSnapshot<App, EFAppEntity>(jsonSerializer, b =>
        {
            b.Property(x => x.IndexedTeamId).HasDomainIdConverter();
        });

        builder.AddSnapshot<Schema, EFSchemaEntity>(jsonSerializer, b =>
        {
            b.Property(x => x.IndexedAppId).HasDomainIdConverter();
            b.Property(x => x.IndexedId).HasDomainIdConverter();
        });

        base.OnModelCreating(builder);
    }
}
