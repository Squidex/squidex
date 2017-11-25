// ==========================================================================
//  BsonConverterTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

#pragma warning disable SA1121 // Use built-in type alias

namespace Squidex.Infrastructure.MongoDb
{
    public class BsonConverterTests
    {
        public class TestObject
        {
            public bool Bool { get; set; }

            public byte Byte { get; set; }

            public byte[] Bytes { get; set; }

            public string String { get; set; }

            public string[] Strings { get; set; }

            public float Float32 { get; set; }

            public double Float64 { get; set; }

            public Uri Uri { get; set; }

            public Guid Guid { get; set; }

            public TimeSpan TimeSpan { get; set; }

            public DateTime DateTime { get; set; }

            public DateTimeOffset DateTimeOffset { get; set; }

            public Int64 Int64 { get; set; }

            public Int32 Int32 { get; set; }

            public Int16 Int16 { get; set; }

            public UInt64 UInt64 { get; set; }

            public UInt32 UInt32 { get; set; }

            public UInt16 UInt16 { get; set; }

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
                    DateTime = DateTime.Today,
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

            target.ShouldBeEquivalentTo(source);
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

                target.ShouldBeEquivalentTo(source);
            }
        }
    }
}
