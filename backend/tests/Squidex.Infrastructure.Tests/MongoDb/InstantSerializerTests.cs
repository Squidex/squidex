// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class InstantSerializerTests
    {
        public InstantSerializerTests()
        {
            InstantSerializer.Register();
        }

        [Fact]
        public void Should_serialize_as_default()
        {
            var source = new Entities.DefaultEntity<Instant> { Value = GetTime() };

            var result1 = SerializeAndDeserializeBson(source);

            Assert.Equal(source.Value, result1.Value);
        }

        [Fact]
        public void Should_serialize_as_string()
        {
            var source = new Entities.StringEntity<Instant> { Value = GetTime() };

            var result1 = SerializeAndDeserializeBson(source);

            Assert.Equal(source.Value, result1.Value);
        }

        [Fact]
        public void Should_serialize_as_int64()
        {
            var source = new Entities.Int64Entity<Instant> { Value = GetTime() };

            var result1 = SerializeAndDeserializeBson(source);

            Assert.Equal(source.Value, result1.Value);
        }

        [Fact]
        public void Should_serialize_as_datetime()
        {
            var source = new Entities.DateTimeEntity<Instant> { Value = GetTime() };

            var result1 = SerializeAndDeserializeBson(source);

            Assert.Equal(source.Value, result1.Value);
        }

        private static Instant GetTime()
        {
            return SystemClock.Instance.GetCurrentInstant().WithoutNs();
        }

        private static T SerializeAndDeserializeBson<T>(T source)
        {
            return SerializeAndDeserializeBson<T, T>(source);
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
