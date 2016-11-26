// ==========================================================================
//  EnrichWithTimestampHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class EnrichWithTimestampHandlerTests
    {
        private sealed class NormalCommand : AggregateCommand
        {
        }

        private sealed class TimestampCommand : AggregateCommand, ITimestampCommand
        {
            public DateTime Timestamp { get; set; }
        }

        [Fact]
        public async Task Should_set_timestamp_for_timestamp_command()
        {
            var utc = DateTime.Today;
            var sut = new EnrichWithTimestampHandler(() => utc);

            var command = new TimestampCommand();

            var result = await sut.HandleAsync(new CommandContext(command));

            Assert.False(result);
            Assert.Equal(utc, command.Timestamp);
        }

        [Fact]
        public async Task Should_set_with_now_datetime_for_timestamp_command()
        {
            var now = DateTime.UtcNow;
            var sut = new EnrichWithTimestampHandler();

            var command = new TimestampCommand();

            var result = await sut.HandleAsync(new CommandContext(command));

            Assert.False(result);
            Assert.True(command.Timestamp >= now && command.Timestamp <= DateTime.UtcNow);
        }

        [Fact]
        public async Task Should_do_nothing_for_normal_command()
        {
            var utc = DateTime.Today;
            var sut = new EnrichWithTimestampHandler(() => utc);

            var result = await sut.HandleAsync(new CommandContext(new NormalCommand()));

            Assert.False(result);
        }
    }
}
