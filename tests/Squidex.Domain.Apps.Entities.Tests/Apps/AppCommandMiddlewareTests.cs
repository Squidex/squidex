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
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppCommandMiddlewareTests : HandlerTestBase<AppState>
    {
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly Context requestContext = new Context();
        private readonly AppCommandMiddleware sut;

        public sealed class MyCommand : SquidexCommand
        {
        }

        protected override Guid Id
        {
            get { return appId; }
        }

        public AppCommandMiddlewareTests()
        {
            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            sut = new AppCommandMiddleware(A.Fake<IGrainFactory>(), contextProvider);
        }

        [Fact]
        public async Task Should_replace_context_app_with_grain_result()
        {
            var result = A.Fake<IAppEntity>();

            var command = CreateCommand(new MyCommand());
            var context = CreateContextForCommand(command);

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(result, requestContext.App);
        }
    }
}
