// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
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
        private readonly IAppRepository appRepository = A.Fake<IAppRepository>();
        private readonly IAppsCacheGrain cache = A.Fake<IAppsCacheGrain>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly string userId = "user1";
        private readonly string clientId = "client1";
        private readonly AppsIndex sut;

        public AppsIndexTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IAppsCacheGrain>(SingleGrain.Id, null))
                .Returns(cache);

            var replicatedCache =
                new ReplicatedCache(new MemoryCache(Options.Create(new MemoryCacheOptions())), new SimplePubSub(A.Fake<ILogger<SimplePubSub>>()),
                    Options.Create(new ReplicatedCacheOptions { Enable = true }));

            sut = new AppsIndex(appRepository, grainFactory, replicatedCache);
        }

        [Fact]
        public async Task Should_resolve_all_apps_from_user_permissions()
        {
            var (expected, _) = CreateApp();

            A.CallTo(() => cache.GetAppIdsAsync(A<string[]>.That.Is(appId.Name)))
                .Returns(new List<DomainId> { appId.Id });

            var actual = await sut.GetAppsForUserAsync(userId, new PermissionSet($"squidex.apps.{appId.Name}"));

            Assert.Same(expected, actual[0]);
        }

        [Fact]
        public async Task Should_resolve_all_apps_from_user()
        {
            var (expected, _) = CreateApp();

            A.CallTo(() => appRepository.QueryIdsAsync(userId, default))
                .Returns(new Dictionary<string, DomainId> { [appId.Name] = appId.Id });

            var actual = await sut.GetAppsForUserAsync(userId, PermissionSet.Empty);

            Assert.Same(expected, actual[0]);
        }

        [Fact]
        public async Task Should_resolve_combined_apps()
        {
            var (expected, _) = CreateApp();

            A.CallTo(() => cache.GetAppIdsAsync(A<string[]>.That.Is(appId.Name)))
                .Returns(new List<DomainId> { appId.Id });

            A.CallTo(() => appRepository.QueryIdsAsync(userId, default))
                .Returns(new Dictionary<string, DomainId> { [appId.Name] = appId.Id });

            var actual = await sut.GetAppsForUserAsync(userId, new PermissionSet($"squidex.apps.{appId.Name}"));

            Assert.Single(actual);
            Assert.Same(expected, actual[0]);
        }

        [Fact]
        public async Task Should_resolve_app_by_name()
        {
            var (expected, _) = CreateApp();

            A.CallTo(() => cache.GetAppIdsAsync(A<string[]>.That.Is(appId.Name)))
                .Returns(new List<DomainId> { appId.Id });

            var actual1 = await sut.GetAppAsync(appId.Name, false);
            var actual2 = await sut.GetAppAsync(appId.Name, false);

            Assert.Same(expected, actual1);
            Assert.Same(expected, actual2);

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(appId.Id.ToString(), null))
                .MustHaveHappenedTwiceExactly();

            A.CallTo(() => cache.GetAppIdsAsync(A<string[]>._))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_resolve_app_by_name_and_id_if_cached_before()
        {
            var (expected, _) = CreateApp();

            A.CallTo(() => cache.GetAppIdsAsync(A<string[]>.That.Is(appId.Name)))
                .Returns(new List<DomainId> { appId.Id });

            var actual1 = await sut.GetAppAsync(appId.Name, true);
            var actual2 = await sut.GetAppAsync(appId.Name, true);
            var actual3 = await sut.GetAppAsync(appId.Id, true);

            Assert.Same(expected, actual1);
            Assert.Same(expected, actual2);
            Assert.Same(expected, actual3);

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(appId.Id.ToString(), null))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => cache.GetAppIdsAsync(A<string[]>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_resolve_app_by_id()
        {
            var (expected, _) = CreateApp();

            var actual1 = await sut.GetAppAsync(appId.Id, false);
            var actual2 = await sut.GetAppAsync(appId.Id, false);

            Assert.Same(expected, actual1);
            Assert.Same(expected, actual2);

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(appId.Id.ToString(), null))
                .MustHaveHappenedTwiceExactly();

            A.CallTo(() => cache.GetAppIdsAsync(A<string[]>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_resolve_app_by_id_and_name_if_cached_before()
        {
            var (expected, _) = CreateApp();

            var actual1 = await sut.GetAppAsync(appId.Id, true);
            var actual2 = await sut.GetAppAsync(appId.Id, true);
            var actual3 = await sut.GetAppAsync(appId.Name, true);

            Assert.Same(expected, actual1);
            Assert.Same(expected, actual2);
            Assert.Same(expected, actual3);

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(appId.Id.ToString(), null))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => cache.GetAppIdsAsync(A<string[]>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_null_if_app_deleted()
        {
            CreateApp(isArchived: true);

            var actual1 = await sut.GetAppAsync(appId.Id, true);
            var actual2 = await sut.GetAppAsync(appId.Id, true);

            Assert.Null(actual1);
            Assert.Null(actual2);
        }

        [Fact]
        public async Task Should_return_null_if_app_not_created()
        {
            CreateApp(EtagVersion.Empty);

            var actual1 = await sut.GetAppAsync(appId.Id, true);
            var actual2 = await sut.GetAppAsync(appId.Id, true);

            Assert.Null(actual1);
            Assert.Null(actual2);
        }

        [Fact]
        public async Task Should_add_app_to_indexes_if_creating()
        {
            var token = RandomHash.Simple();

            A.CallTo(() => cache.ReserveAsync(appId.Id, appId.Name))
                .Returns(token);

            var command = Create(appId.Name);

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => cache.AddAsync(appId.Id, appId.Name))
                .MustHaveHappened();

            A.CallTo(() => cache.RemoveReservationAsync(token))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_clear_reservation_if_app_creation_failed()
        {
            var token = RandomHash.Simple();

            A.CallTo(() => cache.ReserveAsync(appId.Id, appId.Name))
                .Returns(token);

            var command = CreateFromClient(appId.Name);

            var context =
                new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            A.CallTo(() => cache.AddAsync(A<DomainId>._, A<string>._))
                .MustNotHaveHappened();

            A.CallTo(() => cache.RemoveReservationAsync(token))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_add_to_indexes_if_name_is_reserved()
        {
            A.CallTo(() => cache.ReserveAsync(appId.Id, appId.Name))
                .Returns(Task.FromResult<string?>(null));

            var command = Create(appId.Name);

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));

            A.CallTo(() => cache.AddAsync(A<DomainId>._, A<string>._))
                .MustNotHaveHappened();

            A.CallTo(() => cache.RemoveReservationAsync(A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_add_to_indexes_if_name_is_taken()
        {
            var token = RandomHash.Simple();

            A.CallTo(() => cache.ReserveAsync(appId.Id, appId.Name))
                .Returns(token);

            A.CallTo(() => cache.GetAppIdsAsync(A<string[]>.That.Is(appId.Name)))
                .Returns(new List<DomainId> { appId.Id });

            var command = Create(appId.Name);

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));

            A.CallTo(() => cache.AddAsync(A<DomainId>._, A<string>._))
                .MustNotHaveHappened();

            A.CallTo(() => cache.RemoveReservationAsync(token))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_update_index_with_result_if_app_is_updated()
        {
            var (app, appGrain) = CreateApp();

            var command = new UpdateApp { AppId = appId };

            var context =
                new CommandContext(command, commandBus)
                    .Complete(app);

            await sut.HandleAsync(context);

            A.CallTo(() => appGrain.GetStateAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_remove_app_from_indexes_if_app_gets_deleted()
        {
            CreateApp(isArchived: true);

            var command = new DeleteApp { AppId = appId };

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => cache.RemoveAsync(appId.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_reserveration()
        {
            await sut.ReserveAsync(appId.Id, appId.Name);

            A.CallTo(() => cache.ReserveAsync(appId.Id, appId.Name))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_remove_reservation()
        {
            await sut.RemoveReservationAsync("token");

            A.CallTo(() => cache.RemoveReservationAsync("token"))
                .MustHaveHappened();
        }

        private (IAppEntity, IAppGrain) CreateApp(long version = 0, bool fromClient = false, bool isArchived = false)
        {
            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.Id)
                .Returns(appId.Id);
            A.CallTo(() => app.Name)
                .Returns(appId.Name);
            A.CallTo(() => app.Version)
                .Returns(version);
            A.CallTo(() => app.IsDeleted)
                .Returns(isArchived);
            A.CallTo(() => app.Contributors)
                .Returns(AppContributors.Empty.Assign(userId, Role.Owner));

            if (fromClient)
            {
                A.CallTo(() => app.CreatedBy)
                    .Returns(ClientActor());
            }
            else
            {
                A.CallTo(() => app.CreatedBy)
                    .Returns(UserActor());
            }

            var appGrain = A.Fake<IAppGrain>();

            A.CallTo(() => appGrain.GetStateAsync())
                .Returns(J.Of(app));

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(appId.Id.ToString(), null))
                .Returns(appGrain);

            return (app, appGrain);
        }

        private CreateApp Create(string name)
        {
            return new CreateApp { AppId = appId.Id, Name = name, Actor = UserActor() };
        }

        private CreateApp CreateFromClient(string name)
        {
            return new CreateApp { AppId = appId.Id, Name = name, Actor = ClientActor() };
        }

        private RefToken UserActor()
        {
            return RefToken.User(userId);
        }

        private RefToken ClientActor()
        {
            return RefToken.Client(clientId);
        }
    }
}
