// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class BsonJsonSerializerTests
    {
        public class TestWrapper<T>
        {
            [BsonJson]
            public T Value { get; set; }
        }

        public class TestObject
        {
            public bool Bool { get; set; }

            public byte Byte { get; set; }

            public byte[] Bytes { get; set; }

            public int Int32 { get; set; }

            public long Int64 { get; set; }

            public short Int16 { get; set; }

            public uint UInt32 { get; set; }

            public ulong UInt64 { get; set; }

            public ushort UInt16 { get; set; }

            public string String { get; set; }

            public float Float32 { get; set; }

            public double Float64 { get; set; }

            public string[] Strings { get; set; }

            public Uri Uri { get; set; }

            public Guid Guid { get; set; }

            public TimeSpan TimeSpan { get; set; }

            public DateTime DateTime { get; set; }

            public DateTimeOffset DateTimeOffset { get; set; }

            public TestObject Nested { get; set; }

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
            var stream = new MemoryStream();

            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, new TestWrapper<T> { Value = value });
            }

            stream.Position = 0;

            using (var reader = new BsonBinaryReader(stream))
            {
                return BsonSerializer.Deserialize<TestWrapper<T>>(reader)!.Value;
            }
        }
    }
}
