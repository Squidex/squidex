// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Infrastructure.Json.System;
using Squidex.Infrastructure.TestHelpers;
using static Squidex.Infrastructure.Json.Objects.JsonValuesSerializationTests;

namespace Squidex.Infrastructure;

public class NamedIdTests
{
    internal sealed record Wrapper
    {
        [JsonConverter(typeof(StringConverter<NamedId<long>>))]
        public NamedId<long> Value { get; set; }
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
    public void Should_instantiate_token()
    {
        var id = Guid.NewGuid();

        var namedId = NamedId.Of(id, "my-name");

        Assert.Equal(id, namedId.Id);
        Assert.Equal("my-name", namedId.Name);
    }

    [Fact]
    public void Should_convert_named_id_to_string()
    {
        var id = Guid.NewGuid();

        var namedId = NamedId.Of(id, "my-name");

        Assert.Equal($"{id},my-name", namedId.ToString());
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_null_guid_token(SerializerMode mode)
    {
        NamedId<Guid>? value = null;

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_valid_guid_token(SerializerMode mode)
    {
        var value = NamedId.Of(Guid.NewGuid(), "my-name");

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_null_long_token(SerializerMode mode)
    {
        NamedId<long>? value = null;

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_valid_long_token(SerializerMode mode)
    {
        var value = NamedId.Of(123L, "my-name");

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_null_string_token(SerializerMode mode)
    {
        NamedId<string>? value = null;

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_valid_string_token(SerializerMode mode)
    {
        var value = NamedId.Of(Guid.NewGuid().ToString(), "my-name");

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_null_id_token(SerializerMode mode)
    {
        NamedId<DomainId>? value = null;

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Should_serialize_and_deserialize_valid_id_token(SerializerMode mode)
    {
        var value = NamedId.Of(DomainId.NewGuid().ToString(), "my-name");

        var serialized = Serialize(value, mode);

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_old_object()
    {
        var value = new { id = 42L, name = "my-name" };

        var serialized = value.SerializeAndDeserialize<NamedId<long>, object>();

        Assert.Equal(NamedId.Of(42L, "my-name"), serialized);
    }

    [Fact]
    public void Should_deserialize_from_old_object_with_explicit_converter()
    {
        var value = new
        {
            value = new { id = 42, name = "my-name" }
        };

        var expected = new Wrapper
        {
            Value = NamedId.Of(42L, "my-name")
        };

        var serialized = value.SerializeAndDeserialize<Wrapper, object>();

        Assert.Equal(expected, serialized);
    }

    [Fact]
    public void Should_throw_exception_if_string_id_is_not_valid()
    {
        Assert.ThrowsAny<Exception>(() => TestUtils.Deserialize<NamedId<string>>("123"));
    }

    [Fact]
    public void Should_throw_exception_if_long_id_is_not_valid()
    {
        Assert.ThrowsAny<Exception>(() => TestUtils.Deserialize<NamedId<long>>("invalid-long,name"));
    }

    [Fact]
    public void Should_throw_exception_if_guid_id_is_not_valid()
    {
        Assert.ThrowsAny<Exception>(() => TestUtils.Deserialize<NamedId<Guid>>("invalid-guid,name"));
    }
}
