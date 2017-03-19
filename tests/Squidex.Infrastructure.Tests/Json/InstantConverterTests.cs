// ==========================================================================
//  InstantConverterTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Json
{
    public sealed class InstantConverterTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var value = Instant.FromDateTimeUtc(DateTime.UtcNow.Date);

            value.SerializeAndDeserialize(new InstantConverter());
        }
    }
}
