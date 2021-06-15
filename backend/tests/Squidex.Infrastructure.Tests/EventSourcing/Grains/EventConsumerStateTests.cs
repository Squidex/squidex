// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerStateTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var state = new EventConsumerState
            {
                Count = 1,
                IsStopped = true,
                Error = "Error",
                Position = "Position"
            };

            var serialized = state.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(state);
        }
    }
}
