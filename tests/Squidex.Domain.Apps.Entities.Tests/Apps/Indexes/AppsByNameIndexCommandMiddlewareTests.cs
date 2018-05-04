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
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByNameIndexCommandMiddlewareTests
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
            var context =
                new CommandContext(new CreateApp { AppId = appId, Name = "my-app" }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddAppAsync(appId, "my-app"))
                .MustHaveHappened();
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
