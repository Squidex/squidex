// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.MongoDb;

public class DomainIdSerializerTests
{
    private sealed class StringEntity<T>
    {
        [BsonRepresentation(BsonType.String)]
        public T Id { get; set; }
    }

    private sealed class BinaryEntity<T>
    {
        [BsonRepresentation(BsonType.Binary)]
        public T Id { get; set; }
    }

    private sealed class IdEntity<T>
    {
        public T Id { get; set; }
    }

    public DomainIdSerializerTests()
    {
        TestUtils.SetupBson();
    }

    [Fact]
    public void Should_deserialize_from_string()
    {
        var source = new IdEntity<string> { Id = Guid.NewGuid().ToString() };

        var actual = TestUtils.SerializeAndDeserializeBson<IdEntity<DomainId>, IdEntity<string>>(source);

        Assert.Equal(actual.Id.ToString(), source.Id);
    }

    [Fact]
    public void Should_deserialize_from_guid_string()
    {
        var source = new StringEntity<Guid> { Id = Guid.NewGuid() };

        var actual = TestUtils.SerializeAndDeserializeBson<IdEntity<DomainId>, StringEntity<Guid>>(source);

        Assert.Equal(actual.Id.ToString(), source.Id.ToString());
    }

    [Fact]
    public void Should_deserialize_from_guid_bytes()
    {
        var source = new IdEntity<Guid> { Id = Guid.NewGuid() };

        var actual = TestUtils.SerializeAndDeserializeBson<IdEntity<DomainId>, IdEntity<Guid>>(source);

        Assert.Equal(actual.Id.ToString(), source.Id.ToString());
    }

    [Fact]
    public void Should_serialize_guid_as_bytes()
    {
        var source = new BinaryEntity<DomainId> { Id = DomainId.NewGuid() };

        var actual = TestUtils.SerializeAndDeserializeBson(source);

        Assert.Equal(actual.Id, source.Id);
    }

    [Fact]
    public void Should_serialize_non_guid_as_bytes()
    {
        var source = new BinaryEntity<DomainId> { Id = DomainId.Create("NonGuid") };

        var actual = TestUtils.SerializeAndDeserializeBson(source);

        Assert.Equal(actual.Id, source.Id);
    }
}
