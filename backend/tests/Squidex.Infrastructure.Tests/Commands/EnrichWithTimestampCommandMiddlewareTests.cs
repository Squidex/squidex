// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class EnrichWithTimestampCommandMiddlewareTests
    {
        private readonly IClock clock = A.Fake<IClock>();
        private readonly ICommandBus commandBus = A.Dummy<ICommandBus>();
        private readonly EnrichWithTimestampCommandMiddleware sut;

        public EnrichWithTimestampCommandMiddlewareTests()
        {
            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(SystemClock.Instance.GetCurrentInstant().WithoutMs());

            sut = new EnrichWithTimestampCommandMiddleware(clock);
        }

        [Fact]
        public async Task Should_set_timestamp_for_timestamp_command()
        {
            var command = new MyCommand();

            await sut.HandleAsync(new CommandContext(command, commandBus));

            Assert.Equal(clock.GetCurrentInstant(), command.Timestamp);
        }

        [Fact]
        public async Task Should_do_nothing_for_normal_command()
        {
            await sut.HandleAsync(new CommandContext(A.Dummy<ICommand>(), commandBus));

            A.CallTo(() => clock.GetCurrentInstant())
                .MustNotHaveHappened();
        }
    }
}
