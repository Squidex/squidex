// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.TestHelpers;

namespace Squidex.MongoDb.Infrastructure;

public class NamedIdTests
{
    [Fact]
    public void Should_serialize_and_deserialize_null_guid_token()
    {
        NamedId<Guid>? value = null;

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_valid_guid_token()
    {
        var value = NamedId.Of(Guid.NewGuid(), "my-name");

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_null_long_token()
    {
        NamedId<long>? value = null;

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_valid_long_token()
    {
        var value = NamedId.Of(123L, "my-name");

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_null_string_token()
    {
        NamedId<string>? value = null;

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_valid_string_token()
    {
        var value = NamedId.Of(Guid.NewGuid().ToString(), "my-name");

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_null_id_token()
    {
        NamedId<DomainId>? value = null;

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_valid_id_token()
    {
        var value = NamedId.Of(DomainId.NewGuid().ToString(), "my-name");

        var serialized = value.SerializeAndDeserializeBson();

        Assert.Equal(value, serialized);
    }
}
