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
    public sealed class AppsIndexIntegrationTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly AppsByNameIndexGrain byName;
        private readonly AppsByUserIndexGrain byUser;
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly string userId = "user-1";
        private readonly AppsIndex sut;

        public AppsIndexIntegrationTests()
        {
            byName = new AppsByNameIndexGrain(A.Fake<IGrainState<AppsByNameIndexGrain.GrainState>>());
            byName.ActivateAsync(SingleGrain.Id).Wait();

            byUser = new AppsByUserIndexGrain(A.Fake<IGrainState<AppsByUserIndexGrain.GrainState>>());
            byUser.ActivateAsync(userId).Wait();

            A.CallTo(() => grainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id, null))
                .Returns(byName);

            A.CallTo(() => grainFactory.GetGrain<IAppsByUserIndexGrain>(userId, null))
                .Returns(byUser);

            sut = new AppsIndex(grainFactory);
        }

        [Fact]
        public async Task Should_also_add_app_to_index_on_create_when_index_is_inconsistent()
        {
            var appIdOld = Guid.NewGuid();
            var appIdNew = Guid.NewGuid();

            var appName = "my-app";

            SetupApp(appIdOld, appName, -1, false);

            await byName.AddAppAsync(appIdOld, appName);

            var context =
                new CommandContext(Create(appIdNew, appName), commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            var foundIndex = await byName.GetAppIdAsync(appName);

            Assert.Equal(appIdNew, foundIndex);
        }

        private IAppEntity SetupApp(Guid id, string name, long version, bool archived)
        {
            var appEntity = A.Fake<IAppEntity>();

            A.CallTo(() => appEntity.Name)
                .Returns(name);
            A.CallTo(() => appEntity.Version)
                .Returns(version);
            A.CallTo(() => appEntity.IsArchived)
                .Returns(archived);
            A.CallTo(() => appEntity.Contributors)
                .Returns(AppContributors.Empty.Assign(userId, Role.Owner));

            var appGrain = A.Fake<IAppGrain>();

            A.CallTo(() => appGrain.GetStateAsync())
                .Returns(J.Of(appEntity));

            A.CallTo(() => grainFactory.GetGrain<IAppGrain>(id, null))
                .Returns(appGrain);

            return appEntity;
        }

        private CreateApp Create(Guid id, string name)
        {
            return new CreateApp { AppId = id, Name = name, Actor = Actor() };
        }

        private RefToken Actor()
        {
            return new RefToken(RefTokenType.Subject, userId);
        }
    }
}
