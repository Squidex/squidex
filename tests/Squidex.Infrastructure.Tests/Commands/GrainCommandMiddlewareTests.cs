// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class GrainCommandMiddlewareTests
    {
        private readonly IStateFactory factory = A.Fake<IStateFactory>();
        private readonly MyGrain grain = A.Fake<MyGrain>();
        private readonly Guid id = Guid.NewGuid();
        private readonly GrainCommandMiddleware<MatchingCommand, MyGrain> sut;

        public class MatchingCommand : MyCommand
        {
        }

        public GrainCommandMiddlewareTests()
        {
            A.CallTo(() => factory.CreateAsync<MyGrain>(id))
                .Returns(grain);

            sut = new GrainCommandMiddleware<MatchingCommand, MyGrain>(factory);
        }

        [Fact]
        public async Task Should_invoke_grain_when_command_is_correct()
        {
            var command = new MatchingCommand { AggregateId = id };
            var context = new CommandContext(command, A.Fake<ICommandBus>());

            A.CallTo(() => grain.ExecuteAsync(command))
                .Returns(100);

            await sut.HandleAsync(context);

            Assert.Equal(100, context.Result<int>());
        }

        [Fact]
        public async Task Should_not_invoke_grain_when_command_is_not_correct()
        {
            var command = new MyCommand { AggregateId = id };
            var context = new CommandContext(command, A.Fake<ICommandBus>());

            await sut.HandleAsync(context);

            Assert.Null(context.Result<object>());

            A.CallTo(() => grain.ExecuteAsync(command))
                .MustNotHaveHappened();
        }
    }
}
