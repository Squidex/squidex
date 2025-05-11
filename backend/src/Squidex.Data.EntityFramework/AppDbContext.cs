// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Squidex.Assets.TusAdapter;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Contents.Counter;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing.Consume;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;
using YDotNet.Server.EntityFramework;

namespace Squidex;

public abstract class AppDbContext(DbContextOptions options, IJsonSerializer jsonSerializer) : IdentityDbContext(options), IDbContextWithDialect
{
    public abstract SqlDialect Dialect { get; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var jsonColumnType = Dialect.JsonColumnType();

        builder.UseApps(jsonSerializer, jsonColumnType);
        builder.UseAssetKeyValueStore<TusMetadata>();
        builder.UseAssets(jsonSerializer, jsonColumnType);
        builder.UseCache();
        builder.UseChatStore();
        builder.UseContent(jsonSerializer, jsonColumnType, string.Empty);
        builder.UseContentReferences(string.Empty);
        builder.UseContentTables();
        builder.UseCounters(jsonSerializer, jsonColumnType);
        builder.UseCronJobs();
        builder.UseEvents(jsonSerializer, jsonColumnType);
        builder.UseEventStore();
        builder.UseFlows();
        builder.UseHistory(jsonSerializer, jsonColumnType);
        builder.UseIdentity(jsonSerializer, jsonColumnType);
        builder.UseJobs(jsonSerializer, jsonColumnType);
        builder.UseMessagingDataStore();
        builder.UseMessagingTransport();
        builder.UseMigration();
        builder.UseNames(jsonSerializer, jsonColumnType);
        builder.UseOpenIddict();
        builder.UseRequest(jsonSerializer, jsonColumnType);
        builder.UseRules(jsonSerializer, jsonColumnType);
        builder.UseSchema(jsonSerializer, jsonColumnType);
        builder.UseSettings(jsonSerializer, jsonColumnType);
        builder.UseTags(jsonSerializer, jsonColumnType);
        builder.UseTeams(jsonSerializer, jsonColumnType);
        builder.UseTextIndex();
        builder.UseUsage();
        builder.UseUsageTracking(jsonSerializer, jsonColumnType);
        builder.UseYDotNet();

        base.OnModelCreating(builder);
    }
}

#pragma warning disable MA0048 // File name must match type name
internal static class Extensions
#pragma warning restore MA0048 // File name must match type name
{
    public static void UseIdentity(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<DefaultKeyStore.State>(jsonSerializer, jsonColumn);
        builder.UseSnapshot<DefaultXmlRepository.State>(jsonSerializer, jsonColumn);
    }

    public static void UseUsageTracking(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<AssetUsageTracker.State>(jsonSerializer, jsonColumn);
        builder.UseSnapshot<UsageNotifierWorker.State>(jsonSerializer, jsonColumn);
        builder.UseSnapshot<UsageTrackerWorker.State>(jsonSerializer, jsonColumn);
    }

    public static void UseCounters(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<CounterService.State>(jsonSerializer, jsonColumn);
    }

    public static void UseEvents(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<EventConsumerState>(jsonSerializer, jsonColumn);
    }

    public static void UseNames(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<NameReservationState.State>(jsonSerializer, jsonColumn);
    }

    public static void UseJobs(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<JobsState>(jsonSerializer, jsonColumn);
    }

    public static void UseSettings(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<AppUISettings.State>(jsonSerializer, jsonColumn);
    }

    public static void UseTags(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<TagService.State>(jsonSerializer, jsonColumn);
    }
}
