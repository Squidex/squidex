// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class BsonJsonSerializerTests
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

        [Fact]
        public void Should_write_problematic_object()
        {
            var source = new
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

            var deserialized = SerializeAndDeserialize(source);

            deserialized.Should().BeEquivalentTo(source);
        }

        [Fact]
        public void Should_serialize_with_reader_and_writer()
        {
            var source = TestObject.CreateWithValues();

            var deserialized = SerializeAndDeserialize(source);

            deserialized.Should().BeEquivalentTo(source);
        }

        [Fact]
        public void Should_deserialize_property_with_dollar()
        {
            var source = new Dictionary<string, int>
            {
                ["$key"] = 12
            };

            var deserialized = SerializeAndDeserialize(source);

            deserialized.Should().BeEquivalentTo(source);
        }

        [Fact]
        public void Should_deserialize_property_with_dot()
        {
            var source = new Dictionary<string, int>
            {
                ["type.of.value"] = 12
            };

            var deserialized = SerializeAndDeserialize(source);

            deserialized.Should().BeEquivalentTo(source);
        }

        [Fact]
        public void Should_deserialize_property_as_empty_string()
        {
            var source = new Dictionary<string, int>
            {
                [string.Empty] = 12
            };

            var deserialized = SerializeAndDeserialize(source);

            deserialized.Should().BeEquivalentTo(source);
        }

        private static T SerializeAndDeserialize<T>(T value)
        {
            var serializer = JsonSerializer.CreateDefault();

            var stream = new MemoryStream();

            using (var writer = new BsonJsonWriter(new BsonBinaryWriter(stream)))
            {
                serializer.Serialize(writer, value);

                writer.Flush();
            }

            stream.Position = 0;

            using (var reader = new BsonJsonReader(new BsonBinaryReader(stream)))
            {
                return serializer.Deserialize<T>(reader)!;
            }
        }
    }
}
