// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.EventSourcing.Consume;

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
