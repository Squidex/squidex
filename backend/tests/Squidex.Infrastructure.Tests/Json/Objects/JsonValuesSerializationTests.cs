// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.Json.Objects;

public class JsonValuesSerializationTests
{
    public enum SerializerMode
    {
        Json,
        Bson
    }

    public static IEnumerable<object[]> Serializers()
    {
        yield return new object[] { SerializerMode.Json };
        yield return new object[] { SerializerMode.Bson };
    }

    private static T Serialize<T>(T input, SerializerMode mode)
    {
        if (mode == SerializerMode.Bson)
        {
            return input.SerializeAndDeserializeBson();
        }
        else
        {
            return input.SerializeAndDeserialize();
        }
    }

    [Fact]
    public void Should_deserialize_integer()
    {
        var value = 123;

        var serialized = value.SerializeAndDeserialize<JsonValue, int>();

        Assert.Equal(JsonValue.Create(value), serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_null(SerializerMode mode)
    {
        var value = JsonValue.Null;

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_date(SerializerMode mode)
    {
        var value = JsonValue.Create("2008-09-15T15:53:00");

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_string(SerializerMode mode)
    {
        var value = JsonValue.Create("my-string");

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_boolean(SerializerMode mode)
    {
        var value = JsonValue.Create(true);

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_number(SerializerMode mode)
    {
        var value = JsonValue.Create(123);

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_double_number(SerializerMode mode)
    {
        var value = JsonValue.Create(123.5);

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_array(SerializerMode mode)
    {
        var value = JsonValue.Array(1, 2);

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_object(SerializerMode mode)
    {
        var value =
            new JsonObject()
                .Add("1", 1)
                .Add("2", 1);

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_complex_object(SerializerMode mode)
    {
        var value =
            new JsonObject()
                .Add("1",
                    JsonValue.Array(
                        new JsonObject().Add("1_1", 11),
                        new JsonObject().Add("1_2", 12)))
                .Add("2",
                    new JsonObject().Add("2_1", 11));

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_deserialize_from_escaped_dot()
    {
        var value = new Dictionary<string, int>
        {
            ["key.with.dot".JsonToBsonName()] = 10
        };

        var expected =
            new JsonObject()
                .Add("key.with.dot", 10);

        var serialized = TestUtils.SerializeAndDeserializeBson<JsonObject, Dictionary<string, int>>(value);

        Assert.Equal(expected, serialized);
    }
}
