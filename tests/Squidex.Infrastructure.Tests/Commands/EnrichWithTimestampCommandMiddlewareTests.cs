// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public sealed class EnrichWithTimestampCommandMiddlewareTests
    {
        private readonly IClock clock = A.Fake<IClock>();

        [Fact]
        public async Task Should_set_timestamp_for_timestamp_command()
        {
            var utc = Instant.FromUnixTimeSeconds(1000);
            var sut = new EnrichWithTimestampCommandMiddleware(clock);

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(utc);

            var command = new MyCommand();

            await sut.HandleAsync(new CommandContext(command));

            Assert.Equal(utc, command.Timestamp);
        }

        [Fact]
        public async Task Should_do_nothing_for_normal_command()
        {
            var sut = new EnrichWithTimestampCommandMiddleware(clock);

            await sut.HandleAsync(new CommandContext(A.Dummy<ICommand>()));

            A.CallTo(() => clock.GetCurrentInstant()).MustNotHaveHappened();
        }
    }
}
