// ==========================================================================
//  EnrichWithTimestampHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Moq;
using NodaTime;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class EnrichWithTimestampHandlerTests
    {
        private sealed class MyNormalCommand : ICommand
        {
        }

        private sealed class MyTimestampCommand : ITimestampCommand
        {
            public Instant Timestamp { get; set; }
        }

        private readonly Mock<IClock> clock = new Mock<IClock>();

        [Fact]
        public async Task Should_set_timestamp_for_timestamp_command()
        {
            var utc = Instant.FromUnixTimeSeconds(1000);
            var sut = new EnrichWithTimestampHandler(clock.Object);

            clock.Setup(x => x.GetCurrentInstant()).Returns(utc);

            var command = new MyTimestampCommand();

            var result = await sut.HandleAsync(new CommandContext(command));

            Assert.False(result);
            Assert.Equal(utc, command.Timestamp);
        }

        [Fact]
        public async Task Should_do_nothing_for_normal_command()
        {
            var sut = new EnrichWithTimestampHandler(clock.Object);

            var result = await sut.HandleAsync(new CommandContext(new MyNormalCommand()));

            Assert.False(result);

            clock.Verify(x => x.GetCurrentInstant(), Times.Never());
        }
    }
}
