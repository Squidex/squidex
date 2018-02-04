// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
            var value = Instant.FromDateTimeUtc(DateTime.UtcNow.Date);

            value.SerializeAndDeserialize(new InstantConverter());
        }

        [Fact]
        public void Should_serialize_and_deserialize_nullable_with_value()
        {
            Instant? value = Instant.FromDateTimeUtc(DateTime.UtcNow.Date);

            value.SerializeAndDeserialize(new InstantConverter());
        }

        [Fact]
        public void Should_serialize_and_deserialize_nullable_with_null()
        {
            Instant? value = null;

            value.SerializeAndDeserialize(new InstantConverter());
        }
    }
}
