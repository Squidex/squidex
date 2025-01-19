// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;
using Squidex.TestHelpers;

namespace Squidex.MongoDb.Infrastructure;

public class InstantSerializerTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var source = new BsonAsDefaultEntity<Instant>
        {
            Value = GetTime()
        };

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source.Value, deserialized.Value);
    }

    [Fact]
    public void Should_serialize_and_deserialize_as_string()
    {
        var source = new BsonAsStringEntity<Instant>
        {
            Value = GetTime()
        };

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source.Value, deserialized.Value);
    }

    [Fact]
    public void Should_serialize_and_deserialize_as_int64()
    {
        var source = new BsonAsInt64Entity<Instant>
        {
            Value = GetTime()
        };

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source.Value, deserialized.Value);
    }

    [Fact]
    public void Should_serialize_and_deserialize_as_datetime()
    {
        var source = new BsonAsDateTimeEntity<Instant>
        {
            Value = GetTime()
        };

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source.Value, deserialized.Value);
    }

    private static Instant GetTime()
    {
        return SystemClock.Instance.GetCurrentInstant().WithoutNs();
    }
}
