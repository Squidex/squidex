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
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public class AppsByNameIndexCommandMiddlewareTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IAppsByNameIndex index = A.Fake<IAppsByNameIndex>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly AppsByNameIndexCommandMiddleware sut;

        public AppsByNameIndexCommandMiddlewareTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id, null))
                .Returns(index);

            sut = new AppsByNameIndexCommandMiddleware(grainFactory);
        }

        [Fact]
        public async Task Should_add_app_to_index_on_create()
        {
            A.CallTo(() => index.ReserveAppAsync(appId, "my-app"))
                .Returns(true);

            var context =
                new CommandContext(new CreateApp { AppId = appId, Name = "my-app" }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.ReserveAppAsync(appId, "my-app"))
                .MustHaveHappened();

            A.CallTo(() => index.AddAppAsync(appId, "my-app"))
                .MustHaveHappened();

            A.CallTo(() => index.RemoveReservationAsync(appId, "my-app"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_remove_reservation_when_not_reserved()
        {
            A.CallTo(() => index.ReserveAppAsync(appId, "my-app"))
                .Returns(false);

            var context =
                new CommandContext(new CreateApp { AppId = appId, Name = "my-app" }, commandBus)
                    .Complete();

            await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));

            A.CallTo(() => index.ReserveAppAsync(appId, "my-app"))
                .MustHaveHappened();

            A.CallTo(() => index.AddAppAsync(appId, "my-app"))
                .MustNotHaveHappened();

            A.CallTo(() => index.RemoveReservationAsync(appId, "my-app"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_remove_app_from_index_on_archive()
        {
            var context =
                new CommandContext(new ArchiveApp { AppId = appId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveAppAsync(appId))
                .MustHaveHappened();
        }
    }
}
