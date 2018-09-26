// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public class AppsByUserIndexCommandMiddlewareTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IAppsByUserIndex index = A.Fake<IAppsByUserIndex>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly string userId = "123";
        private readonly AppsByUserIndexCommandMiddleware sut;

        public AppsByUserIndexCommandMiddlewareTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IAppsByUserIndex>(userId, null))
                .Returns(index);

            sut = new AppsByUserIndexCommandMiddleware(grainFactory);
        }

        [Fact]
        public async Task Should_add_app_to_index_on_create()
        {
            var context =
                new CommandContext(new CreateApp { AppId = appId, Actor = new RefToken("user", userId) }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddAppAsync(appId))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_add_app_to_index_on_assign_of_contributor()
        {
            var context =
                new CommandContext(new AssignContributor { AppId = appId }, commandBus)
                    .Complete(EntityCreatedResult.Create(userId, 1));

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddAppAsync(appId))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_add_app_to_index_on_remove_of_contributor()
        {
            var context =
                new CommandContext(new RemoveContributor { AppId = appId, ContributorId = userId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveAppAsync(appId))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_app_from_index_on_archive()
        {
            var appGrain = A.Fake<IAppGrain>();
            var appState = A.Fake<IAppEntity>();

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(appId, null))
                .Returns(appGrain);

            A.CallTo(() => appGrain.GetStateAsync())
                .Returns(J.AsTask(appState));

            A.CallTo(() => appState.Contributors)
                .Returns(AppContributors.Empty.Assign(userId, AppContributorPermission.Owner));

            var context =
                new CommandContext(new ArchiveApp { AppId = appId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveAppAsync(appId))
                .MustHaveHappened();
        }
    }
}
