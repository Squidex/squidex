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
    public class SyncedGrainCommandMiddlewareTests
    {
        private readonly IStateFactory factory = A.Fake<IStateFactory>();
        private readonly MyGrain grain = A.Fake<MyGrain>();
        private readonly Guid id = Guid.NewGuid();
        private readonly SyncedGrainCommandMiddleware<MatchingCommand, MyGrain> sut;

        public class MatchingCommand : MyCommand
        {
        }

        public SyncedGrainCommandMiddlewareTests()
        {
            A.CallTo(() => factory.GetSingleAsync<MyGrain>(id))
                .Returns(grain);

            sut = new SyncedGrainCommandMiddleware<MatchingCommand, MyGrain>(factory);
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

            A.CallTo(() => factory.Synchronize<MyGrain, Guid>(id))
                .MustHaveHappened();
            A.CallTo(() => factory.Remove<MyGrain, Guid>(id))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_remove_grain_from_cache_when_failed()
        {
            var command = new MatchingCommand { AggregateId = id };
            var context = new CommandContext(command, A.Fake<ICommandBus>());

            A.CallTo(() => grain.ExecuteAsync(command))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(context));

            A.CallTo(() => factory.Synchronize<MyGrain, Guid>(id))
                .MustNotHaveHappened();
            A.CallTo(() => factory.Remove<MyGrain, Guid>(id))
                .MustHaveHappened();
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
