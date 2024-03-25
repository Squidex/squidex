// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Infrastructure.Validation;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes;

public class AppsIndexTests : GivenContext
{
    private readonly TestState<NameReservationState.State> state;
    private readonly IAppRepository appRepository = A.Fake<IAppRepository>();
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly AppCacheOptions options = new AppCacheOptions();
    private readonly AppsIndex sut;

    public AppsIndexTests()
    {
        options.CacheDuration = TimeSpan.FromMinutes(5);

        state = new TestState<NameReservationState.State>("Apps");

        var replicatedCache =
            new ReplicatedCache(new MemoryCache(Options.Create(new MemoryCacheOptions())), A.Fake<IMessageBus>());

        sut = new AppsIndex(appRepository, replicatedCache, state.PersistenceFactory, Options.Create(options));
    }

    [Fact]
    public async Task Should_resolve_app_by_name()
    {
        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .Returns(App);

        var actual1 = await sut.GetAppAsync(AppId.Name, false, CancellationToken);
        var actual2 = await sut.GetAppAsync(AppId.Name, false, CancellationToken);

        Assert.Same(App, actual1);
        Assert.Same(App, actual2);

        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_app_by_name_and_id_if_cached_before()
    {
        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .ReturnsLazily(() => App with { Version = 3 });

        var actual1 = await sut.GetAppAsync(AppId.Name, true, CancellationToken);
        var actual2 = await sut.GetAppAsync(AppId.Name, true, CancellationToken);
        var actual3 = await sut.GetAppAsync(AppId.Id, true, CancellationToken);

        Assert.Same(actual1, actual2);
        Assert.Same(actual1, actual3);

        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_resolve_app_by_name_and_id_if_cache_before_but_disabled()
    {
        options.CacheDuration = default;

        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .ReturnsLazily(() => App with { Version = 3 });

        var actual1 = await sut.GetAppAsync(AppId.Name, true, CancellationToken);
        var actual2 = await sut.GetAppAsync(AppId.Name, true, CancellationToken);
        var actual3 = await sut.GetAppAsync(AppId.Id, true, CancellationToken);

        Assert.NotSame(actual1, actual2);
        Assert.NotSame(actual1, actual3);

        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_app_by_id()
    {
        A.CallTo(() => appRepository.FindAsync(AppId.Id, CancellationToken))
            .Returns(App);

        var actual1 = await sut.GetAppAsync(AppId.Id, false, CancellationToken);
        var actual2 = await sut.GetAppAsync(AppId.Id, false, CancellationToken);

        Assert.Equal(App, actual1);
        Assert.Equal(App, actual2);

        A.CallTo(() => appRepository.FindAsync(AppId.Id, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_app_by_id_and_name_if_cached_before()
    {
        options.CacheDuration = TimeSpan.FromMinutes(5);

        A.CallTo(() => appRepository.FindAsync(AppId.Id, CancellationToken))
            .ReturnsLazily(() => App with { Version = 3 });

        var actual1 = await sut.GetAppAsync(AppId.Id, true, CancellationToken);
        var actual2 = await sut.GetAppAsync(AppId.Id, true, CancellationToken);
        var actual3 = await sut.GetAppAsync(AppId.Name, true, CancellationToken);

        Assert.Same(actual1, actual2);
        Assert.Same(actual1, actual3);

        A.CallTo(() => appRepository.FindAsync(AppId.Id, CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_resolve_app_by_id_and_name_if_cached_before_but_disabled()
    {
        options.CacheDuration = default;

        A.CallTo(() => appRepository.FindAsync(AppId.Id, CancellationToken))
            .ReturnsLazily(() => App with { Version = 3 });

        var actual1 = await sut.GetAppAsync(AppId.Id, true, CancellationToken);
        var actual2 = await sut.GetAppAsync(AppId.Id, true, CancellationToken);
        var actual3 = await sut.GetAppAsync(AppId.Name, true, CancellationToken);

        Assert.NotSame(actual1, actual2);
        Assert.NotSame(actual1, actual3);

        A.CallTo(() => appRepository.FindAsync(AppId.Id, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_all_apps_from_user_permissions()
    {
        A.CallTo(() => appRepository.QueryAllAsync(User.Identifier, A<IEnumerable<string>>.That.Is(AppId.Name), CancellationToken))
            .Returns([App]);

        var actual = await sut.GetAppsForUserAsync(User.Identifier, new PermissionSet($"squidex.apps.{AppId.Name}"), CancellationToken);

        Assert.Same(App, actual[0]);
    }

    [Fact]
    public async Task Should_resolve_all_apps_from_user()
    {
        A.CallTo(() => appRepository.QueryAllAsync(User.Identifier, A<IEnumerable<string>>.That.IsEmpty(), CancellationToken))
            .Returns([App]);

        var actual = await sut.GetAppsForUserAsync(User.Identifier, PermissionSet.Empty, CancellationToken);

        Assert.Same(App, actual[0]);
    }

    [Fact]
    public async Task Should_resolve_all_apps_from_team()
    {
        A.CallTo(() => appRepository.QueryAllAsync(TeamId, CancellationToken))
            .Returns([App]);

        var actual = await sut.GetAppsForTeamAsync(TeamId, CancellationToken);

        Assert.Same(App, actual[0]);
    }

    [Fact]
    public async Task Should_return_empty_apps_if_app_not_created()
    {
        App = App with { Version = EtagVersion.Empty };

        A.CallTo(() => appRepository.QueryAllAsync(User.Identifier, A<IEnumerable<string>>.That.IsEmpty(), CancellationToken))
            .Returns([App]);

        var actual = await sut.GetAppsForUserAsync(User.Identifier, PermissionSet.Empty, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_empty_apps_if_app_deleted()
    {
        App = App with { IsDeleted = true };

        A.CallTo(() => appRepository.QueryAllAsync(User.Identifier, A<IEnumerable<string>>.That.IsEmpty(), CancellationToken))
            .Returns([App]);

        var actual = await sut.GetAppsForUserAsync(User.Identifier, PermissionSet.Empty, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_take_and_remove_reservation_if_created()
    {
        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .Returns(Task.FromResult<App?>(null));

        var command = Create(AppId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        NameReservation? madeReservation = null;

        await sut.HandleAsync(context, (c, ct) =>
        {
            madeReservation = state.Snapshot.Reservations.FirstOrDefault();

            return Task.CompletedTask;
        }, CancellationToken);

        Assert.Empty(state.Snapshot.Reservations);

        Assert.Equal(AppId.Id, madeReservation?.Id);
        Assert.Equal(AppId.Name, madeReservation?.Name);
    }

    [Fact]
    public async Task Should_clear_reservation_if_app_creation_failed()
    {
        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .Returns(Task.FromResult<App?>(null));

        var command = Create(AppId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        NameReservation? madeReservation = null;

        await Assert.ThrowsAnyAsync<Exception>(() => sut.HandleAsync(context, (c, ct) =>
        {
            madeReservation = state.Snapshot.Reservations.FirstOrDefault();

            throw new InvalidOperationException();
        }, CancellationToken));

        Assert.Empty(state.Snapshot.Reservations);

        Assert.Equal(AppId.Id, madeReservation?.Id);
        Assert.Equal(AppId.Name, madeReservation?.Name);
    }

    [Fact]
    public async Task Should_not_create_app_if_name_is_reserved()
    {
        state.Snapshot.Reservations.Add(new NameReservation(RandomHash.Simple(), AppId.Name, DomainId.NewGuid()));

        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .Returns(Task.FromResult<App?>(null));

        var command = Create(AppId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_not_create_app_if_name_is_taken()
    {
        A.CallTo(() => appRepository.FindAsync(AppId.Name, CancellationToken))
            .Returns(App);

        var command = Create(AppId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context, CancellationToken));

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<NameReservationState.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_make_an_update_for_other_command()
    {
        var command = new UpdateApp { AppId = AppId };

        var context =
            new CommandContext(command, commandBus)
                .Complete(App);

        await sut.HandleAsync(context, CancellationToken);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<NameReservationState.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private CreateApp Create(string name)
    {
        return new CreateApp { AppId = AppId.Id, Name = name };
    }
}
