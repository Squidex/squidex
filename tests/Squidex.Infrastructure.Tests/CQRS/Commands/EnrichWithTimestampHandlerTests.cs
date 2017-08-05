// ==========================================================================
//  EnrichWithTimestampHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class EnrichWithTimestampHandlerTests
    {
        private sealed class MyTimestampCommand : ITimestampCommand
        {
            public Instant Timestamp { get; set; }

            public long? ExpectedVersion { get; set; }
        }

        private readonly IClock clock = A.Fake<IClock>();

        [Fact]
        public async Task Should_set_timestamp_for_timestamp_command()
        {
            var utc = Instant.FromUnixTimeSeconds(1000);
            var sut = new EnrichWithTimestampHandler(clock);

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(utc);

            var command = new MyTimestampCommand();

            var result = await sut.HandleAsync(new CommandContext(command));

            Assert.False(result);
            Assert.Equal(utc, command.Timestamp);
        }

        [Fact]
        public async Task Should_do_nothing_for_normal_command()
        {
            var sut = new EnrichWithTimestampHandler(clock);

            var result = await sut.HandleAsync(new CommandContext(A.Dummy<ICommand>()));

            Assert.False(result);

            A.CallTo(() => clock.GetCurrentInstant()).MustNotHaveHappened();
        }
    }
}
