// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Shared;

public abstract class TeamRepositoryTests
{
    private readonly DomainId knownId = DomainId.Create("3e764e15-3cf5-427f-bb6f-f0fa29a40a2d");

    protected abstract Task<ITeamRepository> CreateSutAsync();

    protected virtual async Task PrepareAsync(ITeamRepository sut, Team[] teams)
    {
        if (sut is not ISnapshotStore<Team> store)
        {
            return;
        }

        var writes = teams.Select(x => new SnapshotWriteJob<Team>(x.Id, x, 0));

        await store.WriteManyAsync(writes);
    }

    private async Task<ITeamRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        if (await sut.FindAsync(knownId) != null)
        {
            return sut;
        }

        var created = SystemClock.Instance.GetCurrentInstant();
        var createdBy = RefToken.Client("client1");

        var known = new Team
        {
            Id = knownId,
            Name = "team1",
            Created = created,
            CreatedBy = createdBy,
        };

        var byAuth = new Team
        {
            Id = DomainId.NewGuid(),
            Name = "by-auth",
            Created = created,
            CreatedBy = createdBy,
            AuthScheme = new AuthScheme { Domain = "squidex.io" },
        };

        var byContributors = new Team
        {
            Id = DomainId.NewGuid(),
            Name = "by-contributor",
            Created = created,
            CreatedBy = createdBy,
            Contributors = Contributors.Empty.Assign("1", Role.Owner).Assign("2", Role.Owner),
        };

        await PrepareAsync(sut, [
            known,
            byAuth,
            byContributors,
        ]);

        return sut;
    }

    [Fact]
    public async Task Should_find_by_id()
    {
        var sut = await CreateAndPrepareSutAsync();

        var found = await sut.FindAsync(knownId);

        Assert.Equal(knownId, found!.Id);
    }

    [Fact]
    public async Task Should_find_by_auth_schema()
    {
        var sut = await CreateAndPrepareSutAsync();

        var found = await sut.FindByAuthDomainAsync("squidex.io");

        Assert.Equal("by-auth", found!.Name);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    public async Task Should_query_by_contributor(string id)
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryAllAsync(id);

        Assert.Equal("by-contributor", result.Single().Name);
    }
}
