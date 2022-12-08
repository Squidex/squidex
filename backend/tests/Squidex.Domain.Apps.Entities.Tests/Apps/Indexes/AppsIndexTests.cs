// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Infrastructure.Validation;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes;

public class AppsIndexTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly TestState<NameReservationState.State> state;
    private readonly IAppRepository appRepository = A.Fake<IAppRepository>();
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly string userId = "user1";
    private readonly AppsIndex sut;

    public AppsIndexTests()
    {
        state = new TestState<NameReservationState.State>("Apps");

        ct = cts.Token;

        var replicatedCache =
            new ReplicatedCache(new MemoryCache(Options.Create(new MemoryCacheOptions())), A.Fake<IMessageBus>(),
                Options.Create(new ReplicatedCacheOptions { Enable = true }));

        sut = new AppsIndex(appRepository, replicatedCache, state.PersistenceFactory);
    }

    [Fact]
    public async Task Should_resolve_app_by_name()
    {
        var expected = CreateApp();

        A.CallTo(() => appRepository.FindAsync(appId.Name, ct))
            .Returns(expected);

        var actual1 = await sut.GetAppAsync(appId.Name, false, ct);
        var actual2 = await sut.GetAppAsync(appId.Name, false, ct);

        Assert.Same(expected, actual1);
        Assert.Same(expected, actual2);

        A.CallTo(() => appRepository.FindAsync(appId.Name, ct))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_app_by_name_and_id_if_cached_before()
    {
        var expected = CreateApp();

        A.CallTo(() => appRepository.FindAsync(appId.Name, ct))
            .Returns(expected);

        var actual1 = await sut.GetAppAsync(appId.Name, true, ct);
        var actual2 = await sut.GetAppAsync(appId.Name, true, ct);
        var actual3 = await sut.GetAppAsync(appId.Id, true, ct);

        Assert.Same(expected, actual1);
        Assert.Same(expected, actual2);
        Assert.Same(expected, actual3);

        A.CallTo(() => appRepository.FindAsync(appId.Name, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_resolve_app_by_id()
    {
        var expected = CreateApp();

        A.CallTo(() => appRepository.FindAsync(appId.Id, ct))
            .Returns(expected);

        var actual1 = await sut.GetAppAsync(appId.Id, false, ct);
        var actual2 = await sut.GetAppAsync(appId.Id, false, ct);

        Assert.Same(expected, actual1);
        Assert.Same(expected, actual2);

        A.CallTo(() => appRepository.FindAsync(appId.Id, ct))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_app_by_id_and_name_if_cached_before()
    {
        var expected = CreateApp();

        A.CallTo(() => appRepository.FindAsync(appId.Id, ct))
            .Returns(expected);

        var actual1 = await sut.GetAppAsync(appId.Id, true, ct);
        var actual2 = await sut.GetAppAsync(appId.Id, true, ct);
        var actual3 = await sut.GetAppAsync(appId.Name, true, ct);

        Assert.Same(expected, actual1);
        Assert.Same(expected, actual2);
        Assert.Same(expected, actual3);

        A.CallTo(() => appRepository.FindAsync(appId.Id, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_resolve_all_apps_from_user_permissions()
    {
        var expected = CreateApp();

        A.CallTo(() => appRepository.QueryAllAsync(userId, A<IEnumerable<string>>.That.Is(appId.Name), ct))
            .Returns(new List<IAppEntity> { expected });

        var actual = await sut.GetAppsForUserAsync(userId, new PermissionSet($"squidex.apps.{appId.Name}"), ct);

        Assert.Same(expected, actual[0]);
    }

    [Fact]
    public async Task Should_resolve_all_apps_from_user()
    {
        var expected = CreateApp();

        A.CallTo(() => appRepository.QueryAllAsync(userId, A<IEnumerable<string>>.That.IsEmpty(), ct))
            .Returns(new List<IAppEntity> { expected });

        var actual = await sut.GetAppsForUserAsync(userId, PermissionSet.Empty, ct);

        Assert.Same(expected, actual[0]);
    }

    [Fact]
    public async Task Should_resolve_all_apps_from_team()
    {
        var teamId = DomainId.NewGuid();

        var expected = CreateApp();

        A.CallTo(() => appRepository.QueryAllAsync(teamId, ct))
            .Returns(new List<IAppEntity> { expected });

        var actual = await sut.GetAppsForTeamAsync(teamId, ct);

        Assert.Same(expected, actual[0]);
    }

    [Fact]
    public async Task Should_return_empty_apps_if_app_not_created()
    {
        var expected = CreateApp(EtagVersion.Empty);

        A.CallTo(() => appRepository.QueryAllAsync(userId, A<IEnumerable<string>>.That.IsEmpty(), ct))
            .Returns(new List<IAppEntity> { expected });

        var actual = await sut.GetAppsForUserAsync(userId, PermissionSet.Empty, ct);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_empty_apps_if_app_deleted()
    {
        var expected = CreateApp(0, true);

        A.CallTo(() => appRepository.QueryAllAsync(userId, A<IEnumerable<string>>.That.IsEmpty(), ct))
            .Returns(new List<IAppEntity> { expected });

        var actual = await sut.GetAppsForUserAsync(userId, PermissionSet.Empty, ct);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_take_and_remove_reservation_if_created()
    {
        A.CallTo(() => appRepository.FindAsync(appId.Name, ct))
            .Returns(Task.FromResult<IAppEntity?>(null));

        var command = Create(appId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        NameReservation? madeReservation = null;

        await sut.HandleAsync(context, (c, ct) =>
        {
            madeReservation = state.Snapshot.Reservations.FirstOrDefault();

            return Task.CompletedTask;
        }, ct);

        Assert.Empty(state.Snapshot.Reservations);

        Assert.Equal(appId.Id, madeReservation?.Id);
        Assert.Equal(appId.Name, madeReservation?.Name);
    }

    [Fact]
    public async Task Should_clear_reservation_if_app_creation_failed()
    {
        A.CallTo(() => appRepository.FindAsync(appId.Name, ct))
            .Returns(Task.FromResult<IAppEntity?>(null));

        var command = Create(appId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        NameReservation? madeReservation = null;

        await Assert.ThrowsAnyAsync<Exception>(() => sut.HandleAsync(context, (c, ct) =>
        {
            madeReservation = state.Snapshot.Reservations.FirstOrDefault();

            throw new InvalidOperationException();
        }, ct));

        Assert.Empty(state.Snapshot.Reservations);

        Assert.Equal(appId.Id, madeReservation?.Id);
        Assert.Equal(appId.Name, madeReservation?.Name);
    }

    [Fact]
    public async Task Should_not_create_app_if_name_is_reserved()
    {
        state.Snapshot.Reservations.Add(new NameReservation(RandomHash.Simple(), appId.Name, DomainId.NewGuid()));

        A.CallTo(() => appRepository.FindAsync(appId.Name, ct))
            .Returns(Task.FromResult<IAppEntity?>(null));

        var command = Create(appId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context, ct));
    }

    [Fact]
    public async Task Should_not_create_app_if_name_is_taken()
    {
        A.CallTo(() => appRepository.FindAsync(appId.Name, ct))
            .Returns(CreateApp());

        var command = Create(appId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context, ct));

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<NameReservationState.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_make_an_update_for_other_command()
    {
        var app = CreateApp();

        var command = new UpdateApp { AppId = appId };

        var context =
            new CommandContext(command, commandBus)
                .Complete(app);

        await sut.HandleAsync(context, ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<NameReservationState.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private CreateApp Create(string name)
    {
        return new CreateApp { AppId = appId.Id, Name = name };
    }

    private IAppEntity CreateApp(long version = 0, bool isDeleted = false)
    {
        var app = A.Fake<IAppEntity>();

        A.CallTo(() => app.Id).Returns(appId.Id);
        A.CallTo(() => app.Name).Returns(appId.Name);
        A.CallTo(() => app.Version).Returns(version);
        A.CallTo(() => app.IsDeleted).Returns(isDeleted);

        return app;
    }
}
