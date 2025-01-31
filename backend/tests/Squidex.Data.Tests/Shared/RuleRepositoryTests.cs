// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Shared;

public abstract class RuleRepositoryTests
{
    private static readonly DomainId KnownId = DomainId.Create("3e764e15-3cf5-427f-bb6f-f0fa29a40a2d");

    protected abstract Task<IRuleRepository> CreateSutAsync();

    protected virtual async Task PrepareAsync(IRuleRepository sut, Rule[] rules)
    {
        if (sut is not ISnapshotStore<Rule> store)
        {
            return;
        }

        var writes = rules.Select(x => new SnapshotWriteJob<Rule>(x.Id, x, 0));

        await store.WriteManyAsync(writes);
    }

    private async Task<IRuleRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        if ((await sut.QueryAllAsync(KnownId)).Count > 0)
        {
            return sut;
        }

        var created = SystemClock.Instance.GetCurrentInstant();
        var createdBy = RefToken.Client("client1");

        var rule1 = new Rule
        {
            AppId = NamedId.Of(KnownId, "my-app"),
            Id = DomainId.NewGuid(),
            Name = "rule1",
            Created = created,
            CreatedBy = createdBy,
        };

        var rule2 = new Rule
        {
            AppId = NamedId.Of(KnownId, "my-app"),
            Id = KnownId,
            Name = "rule2",
            Created = created,
            CreatedBy = createdBy,
        };

        var otherApp = new Rule
        {
            AppId = NamedId.Of(DomainId.NewGuid(), "my-app"),
            Id = DomainId.NewGuid(),
            Name = "rule3",
            Created = created,
            CreatedBy = createdBy,
        };

        await PrepareAsync(sut, [
            rule1,
            rule2,
            otherApp,
        ]);

        return sut;
    }

    [Fact]
    public async Task Should_query_by_app()
    {
        var sut = await CreateAndPrepareSutAsync();

        var found = await sut.QueryAllAsync(KnownId);

        Assert.Equal(2, found.Count);
    }

    [Fact]
    public async Task Should_delete_by_app()
    {
        var sut = await CreateSutAsync();
        if (sut is not IDeleter deleter)
        {
            return;
        }

        var appId = NamedId.Of(DomainId.NewGuid(), "my-app");

        var rule1 = new Rule
        {
            AppId = appId,
            Id = DomainId.NewGuid(),
            Name = "my-rule",
        };

        var rule2 = new Rule
        {
            AppId = appId,
            Id = DomainId.NewGuid(),
            Name = "my-rule",
        };

        await PrepareAsync(sut, [
            rule1,
            rule2,
        ]);

        var found1 = await sut.QueryAllAsync(appId.Id);
        Assert.Equal(2, found1.Count);

        await deleter.DeleteAppAsync(new App { Id = appId.Id }, default);

        var found2 = await sut.QueryAllAsync(appId.Id);
        Assert.Empty(found2);
    }
}
