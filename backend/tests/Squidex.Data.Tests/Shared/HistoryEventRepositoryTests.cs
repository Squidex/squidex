// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Shared;

public abstract class HistoryEventRepositoryTests
{
    private static readonly RefToken Actor = RefToken.Client("client");
    private static readonly DomainId KnownId = DomainId.Create("3e764e15-3cf5-427f-bb6f-f0fa29a40a2d");

    protected abstract Task<IHistoryEventRepository> CreateSutAsync();

    private async Task<IHistoryEventRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        if ((await sut.QueryByChannelAsync(KnownId, string.Empty, 10)).Any())
        {
            return sut;
        }

        var created = SystemClock.Instance.GetCurrentInstant();

        await sut.InsertManyAsync([
            new HistoryEvent
            {
                Id = DomainId.NewGuid(),
                Actor = Actor,
                Channel = "parent1.child1",
                Created = created.Plus(Duration.FromHours(10)),
                EventType = "type",
                OwnerId = KnownId,
            },
            new HistoryEvent
            {
                Id = DomainId.NewGuid(),
                Actor = Actor,
                Channel = "parent1.child2",
                Created = created.Plus(Duration.FromHours(9)),
                EventType = "type",
                OwnerId = KnownId,
            },
            new HistoryEvent
            {
                Id = DomainId.NewGuid(),
                Actor = Actor,
                Channel = "channel",
                Created = created.Plus(Duration.FromHours(6)),
                EventType = "type",
                OwnerId = DomainId.NewGuid(),
            },
        ]);

        return sut;
    }

    [Fact]
    public async Task Query_all_async()
    {
        var result = await QueryAsync(KnownId, string.Empty, 100);

        Assert.Equal(["parent1.child1", "parent1.child2"], result);
    }

    [Fact]
    public async Task Query_with_limited_count()
    {
        var result = await QueryAsync(KnownId, string.Empty, 1);

        Assert.Equal(["parent1.child1"], result);
    }

    [Fact]
    public async Task Query_with_channel()
    {
        var result = await QueryAsync(KnownId, "parent1.child1", 100);

        Assert.Equal(["parent1.child1"], result);
    }

    [Fact]
    public async Task Should_replace_on_insert()
    {
        var sut = await CreateSutAsync();

        var appId = DomainId.NewGuid();

        await sut.InsertManyAsync([
            new HistoryEvent
            {
                Id = appId,
                Actor = Actor,
                Channel = "channel1",
                Created = default,
                EventType = "type",
                OwnerId = appId,
            },
        ]);

        await sut.InsertManyAsync([
            new HistoryEvent
            {
                Id = appId,
                Actor = Actor,
                Channel = "channel2",
                Created = default,
                EventType = "type",
                OwnerId = appId,
            },
        ]);

        var result = await QueryAsync(appId, string.Empty, 100);
        Assert.Equal(["channel2"], result);
    }

    [Fact]
    public async Task Should_delete_by_app()
    {
        var sut = await CreateSutAsync();
        if (sut is not IDeleter deleter)
        {
            return;
        }

        var appId = DomainId.NewGuid();

        await sut.InsertManyAsync([
            new HistoryEvent
            {
                Id = appId,
                Actor = Actor,
                Channel = "channel1",
                Created = default,
                EventType = "type",
                OwnerId = appId,
            },
        ]);

        await deleter.DeleteAppAsync(new App { Id = appId }, default);

        var result = await QueryAsync(appId, string.Empty, 100);
        Assert.Empty(result);
    }

    private async Task<string[]> QueryAsync(DomainId knownId, string prefix, int count)
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryByChannelAsync(knownId, prefix, count);

        return result.Select(x => x.Channel).ToArray();
    }
}
