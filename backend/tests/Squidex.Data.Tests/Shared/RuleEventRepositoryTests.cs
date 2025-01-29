// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http.HttpResults;
using NodaTime;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Shared;

public abstract class RuleEventRepositoryTests
{
    private const int NumValues = 250;
    private readonly DomainId appId;
    private readonly NamedId<DomainId>[] appIds =
    [
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-app1"),
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d453"), "my-app1"),
    ];

    protected RuleEventRepositoryTests()
    {
        appId = appIds[Random.Shared.Next(appIds.Length)].Id;
    }

    protected abstract Task<IRuleEventRepository> CreateSutAsync();

    protected async Task<IRuleEventRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        if ((await sut.QueryByAppAsync(appIds[0].Id, null)).Count > 0)
        {
            return sut;
        }

        var batch = new List<RuleEventWrite>();

        async Task ExecuteBatchAsync(RuleEventWrite? entity)
        {
            if (entity != null)
            {
                batch.Add(entity.Value);
            }

            if ((entity == null || batch.Count >= 1000) && batch.Count > 0)
            {
                await sut.EnqueueAsync(batch);
                batch.Clear();
            }
        }

        var created = SystemClock.Instance.GetCurrentInstant();

        foreach (var forAppId in appIds)
        {
            foreach (var ruleId in appIds)
            {
                for (var i = 0; i < NumValues; i++)
                {
                    await ExecuteBatchAsync(new RuleEventWrite
                    {
                        Job = new RuleJob
                        {
                            Id = DomainId.NewGuid(),
                            ActionData = $"Data{i}",
                            ActionName = $"Action{i}",
                            AppId = forAppId.Id,
                            Created = created,
                            Description = $"Description{i}",
                            EventName = $"Event{i}",
                            ExecutionPartition = 0,
                            Expires = created,
                            RuleId = ruleId.Id,
                        },
                        NextAttempt = created.Plus(Duration.FromDays(i)),
                    });
                }
            }
        }

        await ExecuteBatchAsync(null);
        return sut;
    }

    [Fact]
    public async Task Should_find_by_id()
    {
        var sut = await CreateAndPrepareSutAsync();

        var event1 = (await sut.QueryByAppAsync(appId))[0];
        var event2 = await sut.FindAsync(event1.Id);

        Assert.NotNull(event2);
    }

    [Fact]
    public async Task Should_query_by_app()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryByAppAsync(appId);

        // The default page size is 20.
        Assert.Equal(20, result.Count);
    }

    [Fact]
    public async Task Should_query_by_app_and_rule()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryByAppAsync(appId, appId);

        // The default page size is 20.
        Assert.Equal(20, result.Count);
    }

    [Fact]
    public async Task Should_query_by_app_and_rule_with_count()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryByAppAsync(appId, appId, 0, int.MaxValue);

        // Unlimited count.
        Assert.Equal(NumValues, result.Count);
    }

    [Fact]
    public async Task Should_query_by_app_and_rule_with_offset()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryByAppAsync(appId, appId, NumValues - 5);

        // Only take the last 5 events.
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task Should_query_pending()
    {
        var sut = await CreateAndPrepareSutAsync();

        // Do not return all events.
        var now = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(20));

        var result = await sut.QueryPendingAsync(now).ToListAsync();

        // Result does not return all items.
        Assert.InRange(result.Count, 50, 100);
    }

    [Fact]
    public async Task Should_enqueue_item()
    {
        var sut = await CreateSutAsync();

        var id = DomainId.NewGuid();
        var randomRuleId = DomainId.NewGuid();
        var randomAppId = DomainId.NewGuid();

        await sut.EnqueueAsync([
            new RuleEventWrite
            {
                Job = new RuleJob
                {
                    Id = id,
                    ActionData = $"Data",
                    ActionName = "Action",
                    AppId = randomAppId,
                    Created = SystemClock.Instance.GetCurrentInstant(),
                    Description = $"Description",
                    EventName = "Event",
                    ExecutionPartition = 0,
                    RuleId = randomRuleId,
                },
            },
        ]);

        var nextAttempt = SystemClock.Instance.GetCurrentInstant();
        await sut.EnqueueAsync(id, nextAttempt);

        var found = await sut.FindAsync(id);
        found!.NextAttempt!.Value.Should().BeCloseTo(nextAttempt, Duration.FromSeconds(1));
    }

    [Fact]
    public async Task Should_cancel_by_id()
    {
        var sut = await CreateSutAsync();

        var id = DomainId.NewGuid();
        var randomRuleId = DomainId.NewGuid();
        var randomAppId = DomainId.NewGuid();
        await EnqueueEvent(sut, id, randomRuleId, randomAppId);

        await sut.CancelByEventAsync(id);

        var found = await sut.FindAsync(id);
        Assert.Null(found!.NextAttempt);
        Assert.Equal(RuleJobResult.Cancelled, found.JobResult);
    }

    [Fact]
    public async Task Should_cancel_by_app_id()
    {
        var sut = await CreateSutAsync();

        var id = DomainId.NewGuid();
        var randomRuleId = DomainId.NewGuid();
        var randomAppId = DomainId.NewGuid();
        await EnqueueEvent(sut, id, randomRuleId, randomAppId);

        await sut.CancelByAppAsync(randomAppId);

        var found = await sut.FindAsync(id);
        Assert.Null(found!.NextAttempt);
        Assert.Equal(RuleJobResult.Cancelled, found.JobResult);
    }

    [Fact]
    public async Task Should_cancel_by_rule_id()
    {
        var sut = await CreateSutAsync();

        var id = DomainId.NewGuid();
        var randomRuleId = DomainId.NewGuid();
        var randomAppId = DomainId.NewGuid();
        await EnqueueEvent(sut, id, randomRuleId, randomAppId);

        await sut.CancelByRuleAsync(randomRuleId);

        var found = await sut.FindAsync(id);
        Assert.Null(found!.NextAttempt);
        Assert.Equal(RuleJobResult.Cancelled, found.JobResult);
    }

    private static async Task EnqueueEvent(IRuleEventRepository sut, DomainId id, DomainId ruleId, DomainId appId)
    {
        await sut.EnqueueAsync([
            new RuleEventWrite
            {
                Job = new RuleJob
                {
                    Id = id,
                    ActionData = $"Data",
                    ActionName = "Action",
                    AppId = appId,
                    Created = SystemClock.Instance.GetCurrentInstant(),
                    Description = $"Description",
                    EventName = "Event",
                    ExecutionPartition = 0,
                    RuleId = ruleId,
                },
                NextAttempt = SystemClock.Instance.GetCurrentInstant(),
            },
        ]);
    }
}
