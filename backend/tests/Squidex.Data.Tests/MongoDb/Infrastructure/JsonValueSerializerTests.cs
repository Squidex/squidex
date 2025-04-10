﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;
using Squidex.MongoDb.TestHelpers;

namespace Squidex.MongoDb.Infrastructure;

public class JsonValueSerializerTests
{
    [Fact]
    public void Should_serialize_and_deserialize_null()
    {
        var value = JsonValue.Null;

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_date()
    {
        var value = JsonValue.Create("2008-09-15T15:53:00");

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_string()
    {
        var value = JsonValue.Create("my-string");

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_boolean()
    {
        var value = JsonValue.Create(true);

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_number()
    {
        var value = JsonValue.Create(123);

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_double_number()
    {
        var value = JsonValue.Create(123.5);

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_array()
    {
        var value = JsonValue.Array(1, 2);

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_object()
    {
        var value =
            JsonValue.Object()
                .Add("1", 1)
                .Add("2", 1);

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_complex_object()
    {
        var value =
            JsonValue.Object()
                .Add("1",
                    JsonValue.Array(
                        JsonValue.Object().Add("1_1", 11),
                        JsonValue.Object().Add("1_2", 12)))
                .Add("2",
                    JsonValue.Object().Add("2_1", 11));

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }
}
