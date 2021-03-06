// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class DomainIdSerializerTests
    {
        private sealed class StringEntity<T>
        {
            [BsonRepresentation(BsonType.String)]
            public T Id { get; set; }
        }

        private sealed class IdEntity<T>
        {
            public T Id { get; set; }
        }

        public DomainIdSerializerTests()
        {
            DomainIdSerializer.Register();
        }

        [Fact]
        public void Should_deserialize_from_string()
        {
            var id = Guid.NewGuid();

            var source = new IdEntity<string> { Id = id.ToString() };

            var result = SerializeAndDeserializeBson<IdEntity<string>, IdEntity<DomainId>>(source);

            Assert.Equal(result.Id.ToString(), id.ToString());
        }

        [Fact]
        public void Should_deserialize_from_guid_string()
        {
            var id = Guid.NewGuid();

            var source = new StringEntity<Guid> { Id = id };

            var result = SerializeAndDeserializeBson<StringEntity<Guid>, IdEntity<DomainId>>(source);

            Assert.Equal(result.Id.ToString(), id.ToString());
        }

        [Fact]
        public void Should_deserialize_from_guid_bytes()
        {
            var id = Guid.NewGuid();

            var source = new IdEntity<Guid> { Id = id };

            var result = SerializeAndDeserializeBson<IdEntity<Guid>, IdEntity<DomainId>>(source);

            Assert.Equal(result.Id.ToString(), id.ToString());
        }

        private static TOut SerializeAndDeserializeBson<TIn, TOut>(TIn source)
        {
            var stream = new MemoryStream();

            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, source);

                writer.Flush();
            }

            stream.Position = 0;

            using (var reader = new BsonBinaryReader(stream))
            {
                var target = BsonSerializer.Deserialize<TOut>(reader);

                return target;
            }
        }
    }
}
