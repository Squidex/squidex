// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsIndexTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IAppsByNameIndexGrain byName = A.Fake<IAppsByNameIndexGrain>();
        private readonly IAppsByUserIndexGrain byUser = A.Fake<IAppsByUserIndexGrain>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly string userId = "user-1";
        private readonly AppsIndex sut;

        public AppsIndexTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id, null))
                .Returns(byName);

            A.CallTo(() => grainFactory.GetGrain<IAppsByUserIndexGrain>(userId, null))
                .Returns(byUser);

            sut = new AppsIndex(grainFactory);
        }

        [Fact]
        public async Task Should_resolve_all_apps_from_user_permissions()
        {
            var expected = SetupApp(0, false);

            A.CallTo(() => byName.GetIdsAsync(A<string[]>.That.IsSameSequenceAs(new string[] { appId.Name })))
                .Returns(new List<Guid> { appId.Id });

            var actual = await sut.GetAppsForUserAsync(userId, new PermissionSet($"squidex.apps.{appId.Name}"));

            Assert.Same(expected, actual[0]);
        }

        [Fact]
        public async Task Should_resolve_all_apps_from_user()
        {
            var expected = SetupApp(0, false);

            A.CallTo(() => byUser.GetIdsAsync())
                .Returns(new List<Guid> { appId.Id });

            var actual = await sut.GetAppsForUserAsync(userId, PermissionSet.Empty);

            Assert.Same(expected, actual[0]);
        }

        [Fact]
        public async Task Should_resolve_all_apps()
        {
            var expected = SetupApp(0, false);

            A.CallTo(() => byName.GetIdsAsync())
                .Returns(new List<Guid> { appId.Id });

            var actual = await sut.GetAppsAsync();

            Assert.Same(expected, actual[0]);
        }

        [Fact]
        public async Task Should_resolve_app_by_name()
        {
            var expected = SetupApp(0, false);

            A.CallTo(() => byName.GetIdAsync(appId.Name))
                .Returns(appId.Id);

            var actual = await sut.GetAppAsync(appId.Name);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task Should_resolve_app_by_id()
        {
            var expected = SetupApp(0, false);

            var actual = await sut.GetAppAsync(appId.Id);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task Should_return_null_if_app_archived()
        {
            SetupApp(0, true);

            var actual = await sut.GetAppAsync(appId.Id);

            Assert.Null(actual);
        }

        [Fact]
        public async Task Should_return_null_if_app_not_created()
        {
            SetupApp(-1, false);

            var actual = await sut.GetAppAsync(appId.Id);

            Assert.Null(actual);
        }

        [Fact]
        public async Task Should_clean_index_if_not_consistent()
        {
            SetupApp(-1, false);

            A.CallTo(() => byName.GetIdAsync(appId.Name))
                .Returns(appId.Id);

            await sut.GetAppAsync(appId.Name);

            A.CallTo(() => byName.RemoveAsync(appId.Id)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_add_app_to_index_on_create()
        {
            var token = RandomHash.Simple();

            A.CallTo(() => byName.ReserveAsync(appId.Id, appId.Name))
                .Returns(token);

            var context =
                new CommandContext(Create(appId.Name), commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => byName.AddAsync(token))
                .MustHaveHappened();

            A.CallTo(() => byUser.AddAsync(appId.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_add_to_name_index_on_create_if_name_invalid()
        {
            var context =
                new CommandContext(Create("INVALID"), commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => byName.ReserveAsync(appId.Id, A<string>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => byUser.AddAsync(appId.Id))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_add_app_to_index_on_contributor_assignment()
        {
            var context =
                new CommandContext(new AssignContributor { AppId = appId.Id, ContributorId = userId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => byUser.AddAsync(appId.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_when_app_already_exist()
        {
            A.CallTo(() => byName.ReserveAsync(appId.Id, appId.Name))
                .Returns(Task.FromResult<string>(null));

            var context =
                new CommandContext(Create(appId.Name), commandBus)
                    .Complete();

            await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
        }

        [Fact]
        public async Task Should_remove_from_index_on_remove_of_contributor()
        {
            var context =
                new CommandContext(new RemoveContributor { AppId = appId.Id, ContributorId = userId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => byUser.RemoveAsync(appId.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_app_from_name_index_on_archive()
        {
            var app = SetupApp(0, false);

            var context =
                new CommandContext(new ArchiveApp { AppId = appId.Id }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => byName.RemoveAsync(appId.Id))
                .MustHaveHappened();

            A.CallTo(() => byUser.RemoveAsync(appId.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_call_when_rebuilding_for_contributors1()
        {
            var apps = new HashSet<Guid>();

            await sut.RebuildByContributorsAsync(userId, apps);

            A.CallTo(() => byUser.RebuildAsync(apps))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_call_when_rebuilding_for_contributors2()
        {
            var users = new HashSet<string> { userId };

            await sut.RebuildByContributorsAsync(appId.Id, users);

            A.CallTo(() => byUser.AddAsync(appId.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_call_when_rebuilding()
        {
            var apps = new Dictionary<string, Guid>();

            await sut.RebuildAsync(apps);

            A.CallTo(() => byName.RebuildAsync(apps))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_reserveration()
        {
            await sut.AddAsync("token");

            A.CallTo(() => byName.AddAsync("token"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_remove_reservation()
        {
            await sut.RemoveReservationAsync("token");

            A.CallTo(() => byName.RemoveReservationAsync("token"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_request_for_ids()
        {
            await sut.GetIdsAsync();

            A.CallTo(() => byName.GetIdsAsync())
                .MustHaveHappened();
        }

        private IAppEntity SetupApp(long version, bool archived)
        {
            var appEntity = A.Fake<IAppEntity>();

            A.CallTo(() => appEntity.Name)
                .Returns(appId.Name);
            A.CallTo(() => appEntity.Version)
                .Returns(version);
            A.CallTo(() => appEntity.IsArchived)
                .Returns(archived);
            A.CallTo(() => appEntity.Contributors)
                .Returns(AppContributors.Empty.Assign(userId, Role.Owner));

            var appGrain = A.Fake<IAppGrain>();

            A.CallTo(() => appGrain.GetStateAsync())
                .Returns(J.Of(appEntity));

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(appId.Id, null))
                .Returns(appGrain);

            return appEntity;
        }

        private CreateApp Create(string name)
        {
            return new CreateApp { AppId = appId.Id, Name = name, Actor = Actor() };
        }

        private RefToken Actor()
        {
            return new RefToken(RefTokenType.Subject, userId);
        }
    }
}
