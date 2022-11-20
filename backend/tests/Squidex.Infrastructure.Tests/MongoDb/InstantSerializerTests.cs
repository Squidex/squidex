// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.MongoDb;

public class InstantSerializerTests
{
    public InstantSerializerTests()
    {
        TestUtils.SetupBson();
    }

    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var source = new Entities.DefaultEntity<Instant>
        {
            Value = GetTime()
        };

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source.Value, deserialized.Value);
    }

    [Fact]
    public void Should_serialize_and_deserialize_as_string()
    {
        var source = new Entities.StringEntity<Instant>
        {
            Value = GetTime()
        };

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source.Value, deserialized.Value);
    }

    [Fact]
    public void Should_serialize_and_deserialize_as_int64()
    {
        var source = new Entities.Int64Entity<Instant>
        {
            Value = GetTime()
        };

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source.Value, deserialized.Value);
    }

    [Fact]
    public void Should_serialize_and_deserialize_as_datetime()
    {
        var source = new Entities.DateTimeEntity<Instant>
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
