// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class BsonUniqueContentIdSerializerTests
{
    public BsonUniqueContentIdSerializerTests()
    {
        BsonUniqueContentIdSerializer.Register();
    }

    [Fact]
    public void Should_serialize_and_deserialize_guid_guid()
    {
        var source = new UniqueContentId(DomainId.NewGuid(), DomainId.NewGuid());

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_guid_custom()
    {
        var source = new UniqueContentId(DomainId.NewGuid(), DomainId.Create("id42"));

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_guid_empty()
    {
        var source = new UniqueContentId(DomainId.NewGuid(), DomainId.Empty);

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_custom_custom()
    {
        var source = new UniqueContentId(DomainId.Create("id41"), DomainId.Create("id42"));

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_custom_guid()
    {
        var source = new UniqueContentId(DomainId.Create("id42"), DomainId.NewGuid());

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }

    [Fact]
    public void Should_calculate_next_custom_id()
    {
        var appId = DomainId.Create("x");

        var actual = BsonUniqueContentIdSerializer.NextAppId(appId);

        Assert.Equal(new UniqueContentId(DomainId.Create("y"), DomainId.Empty), actual);
    }

    [Fact]
    public void Should_calculate_next_guid_id()
    {
        var appId = DomainId.Create("70fb3772-2ab5-4854-b421-054d2479a0f7");

        var actual = BsonUniqueContentIdSerializer.NextAppId(appId);

        Assert.Equal(new UniqueContentId(DomainId.Create("70fb3773-2ab5-4854-b421-054d2479a0f7"), DomainId.Empty), actual);
    }
}
