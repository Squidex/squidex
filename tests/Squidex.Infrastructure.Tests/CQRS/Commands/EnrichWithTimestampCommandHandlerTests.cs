// ==========================================================================
//  EnrichWithTimestampCommandHandlerTests.cs
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
    public sealed class EnrichWithTimestampCommandHandlerTests
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

            await sut.HandleAsync(new CommandContext(command));

            Assert.Equal(utc, command.Timestamp);
        }

        [Fact]
        public async Task Should_do_nothing_for_normal_command()
        {
            var sut = new EnrichWithTimestampHandler(clock);

            await sut.HandleAsync(new CommandContext(A.Dummy<ICommand>()));

            A.CallTo(() => clock.GetCurrentInstant()).MustNotHaveHappened();
        }
    }
}
