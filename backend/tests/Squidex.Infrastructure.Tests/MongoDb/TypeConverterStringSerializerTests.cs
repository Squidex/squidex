// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class TypeConverterStringSerializerTests
    {
        private sealed record ValueHolder<T>
        {
            public T Value { get; set; }
        }

        public TypeConverterStringSerializerTests()
        {
            TypeConverterStringSerializer<TimeSpan>.Register();
            TypeConverterStringSerializer<RefToken>.Register();
        }

        [Fact]
        public void Should_serialize_struct()
        {
            var source = new ValueHolder<TimeSpan>
            {
                Value = TimeSpan.Zero
            };

            var deserialized = SerializeAndDeserializeBson(source);

            Assert.Equal(source, deserialized);
        }

        [Fact]
        public void Should_serialize_nullable_struct()
        {
            var source = new ValueHolder<TimeSpan?>
            {
                Value = TimeSpan.Zero
            };

            var deserialized = SerializeAndDeserializeBson(source);

            Assert.Equal(source, deserialized);
        }

        [Fact]
        public void Should_serialize_nullable_null_struct()
        {
            var source = new ValueHolder<TimeSpan?>
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
            var stream = new MemoryStream();

            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, value);

                writer.Flush();
            }

            stream.Position = 0;

            using (var reader = new BsonBinaryReader(stream))
            {
                var result = BsonSerializer.Deserialize<T>(reader);

                return result;
            }
        }
    }
}
