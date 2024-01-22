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

    public static readonly TheoryData<string> CustomIds = new TheoryData<string>
    {
        "id",
        "id-short",
        "id-with-very-long-text",
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
    };

    [Fact]
    public void Should_serialize_and_deserialize_guid_guid()
    {
        var source = new UniqueContentId(DomainId.NewGuid(), DomainId.NewGuid());

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_guid_guid2()
    {
        var source = new UniqueContentId(DomainId.Create("97432068-10f9-4b98-81ba-ef93a96cc466"), DomainId.Create("1586987c-9540-421a-9cc9-3381c3f4109f"));

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }

    [Theory]
    [MemberData(nameof(CustomIds))]
    public void Should_serialize_and_deserialize_guid_custom(string id)
    {
        var source = new UniqueContentId(DomainId.NewGuid(), DomainId.Create(id));

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

    [Theory]
    [MemberData(nameof(CustomIds))]
    public void Should_serialize_and_deserialize_custom_custom(string id)
    {
        var source = new UniqueContentId(DomainId.Create(id), DomainId.Create(id));

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }

    [Theory]
    [MemberData(nameof(CustomIds))]
    public void Should_serialize_and_deserialize_custom_guid(string id)
    {
        var source = new UniqueContentId(DomainId.Create(id), DomainId.NewGuid());

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
