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
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex;

public class AppDbContext(DbContextOptions options, IJsonSerializer jsonSerializer) : IdentityDbContext(options)
{
    public DbSet<EFAppEntity> Apps { get; set; }

    public DbSet<EFCacheEntity> Cache { get; set; }

    public DbSet<EFUsageCounterEntity> Counters { get; set; }

    public DbSet<EFRequestEntity> Log { get; set; }

    public DbSet<HistoryEvent> History { get; set; }

    public DbSet<EFMigrationEntity> Migrations { get; set; }

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
            b.Property(x => x.IndexedTeamId).AsString();
        });

        builder.AddSnapshot<Rule, EFRuleEntity>(jsonSerializer, b =>
        {
            b.Property(x => x.IndexedAppId).AsString();
            b.Property(x => x.IndexedId).AsString();
        });

        builder.AddSnapshot<Schema, EFSchemaEntity>(jsonSerializer, b =>
        {
            b.Property(x => x.IndexedAppId).AsString();
            b.Property(x => x.IndexedId).AsString();
        });

        builder.Entity<HistoryEvent>(b =>
        {
            b.Property(x => x.Actor).AsString();
            b.Property(x => x.Id).AsString();
            b.Property(x => x.OwnerId).AsString();
            b.Property(x => x.Parameters).AsJsonString(jsonSerializer);
            b.Property(x => x.Created).AsDateTimeOffset();
        });

        builder.Entity<EFRequestEntity>(b =>
        {
            b.Property(x => x.Timestamp).AsDateTimeOffset();
            b.Property(x => x.Properties).AsJsonString(jsonSerializer);
        });

        builder.Entity<EFAssetEntity>(b =>
        {
            b.Property(x => x.Id).AsString();
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.Created).AsDateTimeOffset();
            b.Property(x => x.CreatedBy).AsString();
            b.Property(x => x.DocumentId).AsString();
            b.Property(x => x.IndexedAppId).AsString();
            b.Property(x => x.LastModified).AsDateTimeOffset();
            b.Property(x => x.LastModifiedBy).AsString();
            b.Property(x => x.Metadata).AsJsonString(jsonSerializer);
            b.Property(x => x.ParentId).AsString();
            b.Property(x => x.Tags).AsString();
            b.Property(x => x.Type).AsString();
        });

        builder.Entity<EFAssetFolderEntity>(b =>
        {
            b.Property(x => x.Id).AsString();
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.Created).AsDateTimeOffset();
            b.Property(x => x.CreatedBy).AsString();
            b.Property(x => x.DocumentId).AsString();
            b.Property(x => x.IndexedAppId).AsString();
            b.Property(x => x.LastModified).AsDateTimeOffset();
            b.Property(x => x.LastModifiedBy).AsString();
            b.Property(x => x.ParentId).AsString();
        });

        builder.Entity<EFRuleEventEntity>(b =>
        {
            b.Property(x => x.Id).AsString();
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.Created).AsDateTimeOffset();
            b.Property(x => x.Expires).AsDateTimeOffset();
            b.Property(x => x.Job).AsJsonString(jsonSerializer);
            b.Property(x => x.JobResult).AsString();
            b.Property(x => x.LastModified).AsDateTimeOffset();
            b.Property(x => x.NextAttempt).AsDateTimeOffset();
            b.Property(x => x.Result).AsString();
            b.Property(x => x.RuleId).AsString();
        });

        base.OnModelCreating(builder);
    }
}
