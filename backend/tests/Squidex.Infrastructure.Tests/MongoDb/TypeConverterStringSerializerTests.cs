// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class TypeConverterStringSerializerTests
    {
        public sealed record ValueHolder<T>
        {
            public T Value { get; set; }
        }

        public TypeConverterStringSerializerTests()
        {
            TypeConverterStringSerializer<DomainId>.Register();
            TypeConverterStringSerializer<RefToken>.Register();
        }

        [Fact]
        public void Should_serialize_struct()
        {
            var source = new ValueHolder<DomainId>
            {
                Value = DomainId.NewGuid()
            };

            var deserialized = SerializeAndDeserializeBson(source);

            Assert.Equal(source, deserialized);
        }

        [Fact]
        public void Should_serialize_nullable_struct()
        {
            var source = new ValueHolder<DomainId?>
            {
                Value = DomainId.NewGuid()
            };

            var deserialized = SerializeAndDeserializeBson(source);

            Assert.Equal(source, deserialized);
        }

        [Fact]
        public void Should_serialize_nullable_null_struct()
        {
            var source = new ValueHolder<DomainId?>
            {
                Value = null
            };

            var deserialized = SerializeAndDeserializeBson(source);

            Assert.Equal(source, deserialized);
        }

        [Fact]
        public void Should_serialize_class()
        {
            var source = new ValueHolder<RefToken>
            {
                Value = RefToken.Client("client")
            };

            var deserialized = SerializeAndDeserializeBson(source);

            Assert.Equal(source, deserialized);
        }

        [Fact]
        public void Should_serialize_null_class()
        {
            var source = new ValueHolder<RefToken?>
            {
                Value = null
            };

            var deserialized = SerializeAndDeserializeBson(source);

            Assert.Equal(source, deserialized);
        }

        private static T SerializeAndDeserializeBson<T>(T value)
        {
            var document = new BsonDocument();

            using (var writer = new BsonDocumentWriter(document))
            {
                BsonSerializer.Serialize(writer, value);

                writer.Flush();
            }

            using (var reader = new BsonDocumentReader(document))
            {
                var result = BsonSerializer.Deserialize<T>(reader);

                return result;
            }
        }
    }
}
