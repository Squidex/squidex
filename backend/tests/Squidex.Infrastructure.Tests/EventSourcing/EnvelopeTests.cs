// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class EnvelopeTests
    {
        public class MyEvent : IEvent
        {
            public int Value { get; set; }
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var value = Envelope.Create(new MyEvent { Value = 1 });

            var deserialized = value.SerializeAndDeserialize();

            Assert.Equal(1, deserialized.Payload.Value);
        }
    }
}
