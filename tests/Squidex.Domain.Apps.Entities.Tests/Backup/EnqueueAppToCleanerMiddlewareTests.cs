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

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class EnqueueAppToCleanerMiddlewareTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IAppCleanerGrain index = A.Fake<IAppCleanerGrain>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly EnqueueAppToCleanerMiddleware sut;

        public EnqueueAppToCleanerMiddlewareTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IAppCleanerGrain>(SingleGrain.Id, null))
                .Returns(index);

            sut = new EnqueueAppToCleanerMiddleware(grainFactory);
        }

        [Fact]
        public async Task Should_enqueue_for_cleanup_on_archive()
        {
            var context =
                new CommandContext(new ArchiveApp { AppId = appId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.EnqueueAppAsync(appId))
                .MustHaveHappened();
        }
    }
}
