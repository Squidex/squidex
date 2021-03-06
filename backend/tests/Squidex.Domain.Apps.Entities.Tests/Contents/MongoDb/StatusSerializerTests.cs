// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.MongoDb;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb
{
    public sealed class StatusSerializerTests
    {
        private sealed record ValueHolder<T>
        {
            public T Value { get; set; }
        }

        public StatusSerializerTests()
        {
            TypeConverterStringSerializer<Status>.Register();
        }

        [Fact]
        public void Should_serialize_and_deserialize_status()
        {
            var source = new ValueHolder<Status>
            {
                Value = Status.Published
            };

            var deserialized = SerializeAndDeserializeBson(source);

            Assert.Equal(source, deserialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_default_status()
        {
            var source = new ValueHolder<Status>
            {
                Value = default
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
