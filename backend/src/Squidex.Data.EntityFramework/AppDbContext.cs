// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Data.EntityFramework;

public class AppDbContext(DbContextOptions options, IJsonSerializer jsonSerializer) : IdentityDbContext(options)
{
    public DbSet<EFAppEntity> Apps { get; set; }

    public DbSet<EFUsageCounterEntity> Counters { get; set; }

    public DbSet<EFRuleEntity> Rules { get; set; }

    public DbSet<EFSchemaEntity> Schemas { get; set; }

    public DbSet<EFTeamEntity> Teams { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseOpenIddict();

        builder.AddChatStore();
        builder.AddEventStore();
        builder.AddMessagingDataStore();
        builder.AddMessagingTransport();

        builder.AddSnapshot<Team, EFTeamEntity>(jsonSerializer);

        builder.AddSnapshot<App, EFAppEntity>(jsonSerializer, b =>
        {
            b.Property(x => x.IndexedTeamId).HasDomainIdConverter();
        });

        builder.AddSnapshot<Rule, EFRuleEntity>(jsonSerializer, b =>
        {
            b.Property(x => x.IndexedAppId).HasDomainIdConverter();
            b.Property(x => x.IndexedId).HasDomainIdConverter();
        });

        builder.AddSnapshot<Schema, EFSchemaEntity>(jsonSerializer, b =>
        {
            b.Property(x => x.IndexedAppId).HasDomainIdConverter();
            b.Property(x => x.IndexedId).HasDomainIdConverter();
        });

        base.OnModelCreating(builder);
    }
}
