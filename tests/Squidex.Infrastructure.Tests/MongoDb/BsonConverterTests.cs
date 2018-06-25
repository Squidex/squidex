// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class BsonConverterTests
    {
        public class TestObject
        {
            [JsonProperty]
            public bool Bool { get; set; }

            [JsonProperty]
            public byte Byte { get; set; }

            [JsonProperty]
            public byte[] Bytes { get; set; }

            [JsonProperty]
            public int Int32 { get; set; }

            [JsonProperty]
            public long Int64 { get; set; }

            [JsonProperty]
            public short Int16 { get; set; }

            [JsonProperty]
            public uint UInt32 { get; set; }

            [JsonProperty]
            public ulong UInt64 { get; set; }

            [JsonProperty]
            public ushort UInt16 { get; set; }

            [JsonProperty]
            public string String { get; set; }

            [JsonProperty]
            public float Float32 { get; set; }

            [JsonProperty]
            public double Float64 { get; set; }

            [JsonProperty]
            public string[] Strings { get; set; }

            [JsonProperty]
            public Uri Uri { get; set; }

            [JsonProperty]
            public Guid Guid { get; set; }

            [JsonProperty]
            public TimeSpan TimeSpan { get; set; }

            [JsonProperty]
            public DateTime DateTime { get; set; }

            [JsonProperty]
            public DateTimeOffset DateTimeOffset { get; set; }

            [JsonProperty]
            public TestObject Nested { get; set; }

            [JsonProperty]
            public TestObject[] NestedArray { get; set; }

            public static TestObject CreateWithValues(bool nested = true)
            {
                var result = new TestObject
                {
                    Bool = true,
                    Byte = 0x2,
                    Bytes = new byte[] { 0x10, 0x12, 0x13 },
                    DateTimeOffset = DateTime.Today,
                    DateTime = DateTime.UtcNow.Date,
                    Float32 = 32.5f,
                    Float64 = 32.5d,
                    Guid = Guid.NewGuid(),
                    Int64 = 64,
                    Int32 = 32,
                    Int16 = 16,
                    String = "squidex",
                    Strings = new[] { "hello", "squidex " },
                    TimeSpan = TimeSpan.FromSeconds(123),
                    UInt64 = 164,
                    UInt32 = 132,
                    UInt16 = 116,
                    Uri = new Uri("http://squidex.io")
                };

                if (nested)
                {
                    result.Nested = CreateWithValues(false);
                    result.NestedArray = Enumerable.Repeat(0, 4).Select(x => CreateWithValues(false)).ToArray();
                }

                return result;
            }
        }

        private readonly TestObject source = TestObject.CreateWithValues();
        private readonly JsonSerializer serializer = JsonSerializer.CreateDefault();

        [Fact]
        public void Should_serialize_and_deserialize_to_bson_with_json()
        {
            var target = JObject.FromObject(source).ToBson().ToJson().ToObject<TestObject>();

            target.Should().BeEquivalentTo(source);
        }

        [Fact]
        public void Should_serialize_datetime_to_iso()
        {
            source.DateTime = new DateTime(2012, 12, 12, 12, 12, 12, DateTimeKind.Utc);

            var target = JObject.FromObject(source).ToBson();

            Assert.Equal("2012-12-12T12:12:12Z", target["DateTime"].ToString());
        }

        [Fact]
        public void Should_serialize_datetimeoffset_to_iso_utc()
        {
            source.DateTimeOffset = new DateTime(2012, 12, 12, 12, 12, 12, DateTimeKind.Utc);

            var target = JObject.FromObject(source).ToBson();

            Assert.Equal("2012-12-12T12:12:12Z", target["DateTimeOffset"].ToString());
        }

        [Fact]
        public void Should_serialize_datetimeoffset_to_iso_utc_with_offset()
        {
            source.DateTimeOffset = new DateTimeOffset(2012, 12, 12, 12, 12, 12, TimeSpan.FromHours(2));

            var target = JObject.FromObject(source).ToBson();

            Assert.Equal("2012-12-12T12:12:12+02:00", target["DateTimeOffset"].ToString());
        }

        [Fact]
        public void Should_write_problematic_object()
        {
            var buggy = new
            {
                a = new
                {
                    iv = 1
                },
                b = new
                {
                    iv = JObject.FromObject(new
                    {
                        lat = 1.0,
                        lon = 3.0
                    })
                }
            };

            var stream = new MemoryStream();

            using (var writer = new BsonJsonWriter(new BsonBinaryWriter(stream)))
            {
                serializer.Serialize(writer, buggy);

                writer.Flush();
            }

            stream.Position = 0;

            using (var reader = new BsonJsonReader(new BsonBinaryReader(stream)))
            {
                var target = serializer.Deserialize(reader, buggy.GetType());

                target.Should().BeEquivalentTo(buggy);
            }
        }

        [Fact]
        public void Should_serialize_with_reader_and_writer()
        {
            var stream = new MemoryStream();

            using (var writer = new BsonJsonWriter(new BsonBinaryWriter(stream)))
            {
                serializer.Serialize(writer, source);

                writer.Flush();
            }

            stream.Position = 0;

            using (var reader = new BsonJsonReader(new BsonBinaryReader(stream)))
            {
                var target = serializer.Deserialize<TestObject>(reader);

                target.Should().BeEquivalentTo(source);
            }
        }
    }
}
