// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Shared;

public abstract class AppRepositoryTests
{
    private static readonly DomainId KnownId = DomainId.Create("3e764e15-3cf5-427f-bb6f-f0fa29a40a2d");

    protected abstract Task<IAppRepository> CreateSutAsync();

    protected virtual async Task PrepareAsync(IAppRepository sut, App[] apps)
    {
        if (sut is not ISnapshotStore<App> store)
        {
            return;
        }

        var writes = apps.Select(x => new SnapshotWriteJob<App>(x.Id, x, 0));

        await store.WriteManyAsync(writes);
    }

    private async Task<IAppRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        if (await sut.FindAsync(KnownId) != null)
        {
            return sut;
        }

        var created = SystemClock.Instance.GetCurrentInstant();
        var createdBy = RefToken.Client("client1");

        var defaultNotDeleted = new App
        {
            Id = KnownId,
            Name = "default",
            Description = "default-not-deleted",
            IsDeleted = false,
            Created = created,
            CreatedBy = createdBy,
        };

        var defaultDeleted = new App
        {
            Id = DomainId.NewGuid(),
            Name = "default",
            Description = "default-deleted",
            IsDeleted = true,
            Created = created,
            CreatedBy = createdBy,
        };

        var duplicateName1 = new App
        {
            Id = DomainId.NewGuid(),
            Name = "with-duplicate",
            Description = "duplicate-old",
            Created = created.Minus(Duration.FromHours(1)),
            CreatedBy = createdBy,
        };

        var duplicateName2 = new App
        {
            Id = DomainId.NewGuid(),
            Name = "with-duplicate",
            Description = "duplicate-new",
            Created = created,
            CreatedBy = createdBy,
        };

        var byTeam = new App
        {
            Id = DomainId.NewGuid(),
            Name = "by-team",
            Description = "by-team",
            TeamId = DomainId.Create(Guid.Empty),
            Created = created,
            CreatedBy = createdBy,
        };

        var byContributors = new App
        {
            Id = DomainId.NewGuid(),
            Name = "by-contributor",
            Description = "by-contributor",
            Contributors = Contributors.Empty.Assign("1", Role.Owner).Assign("2", Role.Owner),
            Created = created,
            CreatedBy = createdBy,
        };

        await PrepareAsync(sut, [
            defaultDeleted,
            defaultNotDeleted,
            duplicateName1,
            duplicateName2,
            byTeam,
            byContributors,
        ]);

        return sut;
    }

    [Fact]
    public async Task Should_query_by_team()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryAllAsync(DomainId.Create(Guid.Empty));

        Assert.Equal("by-team", result.Single().Description);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    public async Task Should_query_by_contributor(string id)
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryAllAsync(id, []);

        Assert.Equal("by-contributor", result.Single().Description);
    }

    [Fact]
    public async Task Should_query_by_duplicate_name()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryAllAsync(string.Empty, ["with-duplicate"]);

        Assert.Equal("duplicate-new", result.Single().Description);
    }

    [Fact]
    public async Task Should_find_by_duplicate_name()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.FindAsync("with-duplicate");

        Assert.Equal("duplicate-new", result?.Description);
    }

    [Fact]
    public async Task Should_find_by_name()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.FindAsync("default");

        Assert.Equal("default-not-deleted", result?.Description);
    }

    [Fact]
    public async Task Should_find_by_id()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.FindAsync(KnownId);

        Assert.Equal("default-not-deleted", result?.Description);
    }
}
