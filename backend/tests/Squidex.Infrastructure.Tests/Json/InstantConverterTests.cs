// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Json
{
    public class InstantConverterTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var value = Instant.FromUtc(2012, 12, 10, 9, 8, 45);

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_nullable_with_value()
        {
            Instant? value = Instant.FromUtc(2012, 12, 10, 9, 8, 45);

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_nullable_with_null()
        {
            Instant? value = null;

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }
    }
}
