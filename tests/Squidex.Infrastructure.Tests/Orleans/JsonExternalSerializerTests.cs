// ==========================================================================
//  JsonExternalSerializerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using FakeItEasy;
using Orleans.Serialization;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public class JsonExternalSerializerTests
    {
        [Fact]
        public void Should_not_copy_null()
        {
            var v = (string)null;
            var c = J<int>.Copy(v, null);

            Assert.Null(c);
        }

        [Fact]
        public void Should_copy_null_json()
        {
            var v = new J<List<int>>(null);
            var c = (J<List<int>>)J<object>.Copy(v, null);

            Assert.Null(c.Value);
        }

        [Fact]
        public void Should_not_copy_immutable_values()
        {
            var v = new List<int> { 1, 2, 3 }.AsJ();
            var c = (J<List<int>>)J<object>.Copy(v, null);

            Assert.Same(v.Value, c.Value);
        }

        [Fact]
        public void Should_serialize_and_deserialize_value()
        {
            SerializeAndDeserialize(ArrayOfLength(100), Assert.Equal);
        }

        [Fact]
        public void Should_serialize_and_deserialize_large_value()
        {
            SerializeAndDeserialize(ArrayOfLength(8000), Assert.Equal);
        }

        private static void SerializeAndDeserialize<T>(T value, Action<T, T> equals) where T : class
        {
            var buffer = new MemoryStream();

            J<object>.Serialize(J.Of(value), CreateWriter(buffer), typeof(T));

            buffer.Position = 0;

            var copy = (J<T>)J<object>.Deserialize(typeof(J<T>), CreateReader(buffer));

            equals(copy.Value, value);

            Assert.NotSame(value, copy.Value);
        }

        private static DeserializationContext CreateReader(MemoryStream buffer)
        {
            var reader = A.Fake<IBinaryTokenStreamReader>();

            A.CallTo(() => reader.ReadByteArray(A<byte[]>.Ignored, A<int>.Ignored, A<int>.Ignored))
                .Invokes(new Action<byte[], int, int>((b, o, l) => buffer.Read(b, o, l)));
            A.CallTo(() => reader.CurrentPosition)
                .ReturnsLazily(x => (int)buffer.Position);
            A.CallTo(() => reader.Length)
                .ReturnsLazily(x => (int)buffer.Length);

            return new DeserializationContext(null) { StreamReader = reader };
        }

        private static SerializationContext CreateWriter(MemoryStream buffer)
        {
            var writer = A.Fake<IBinaryTokenStreamWriter>();

            A.CallTo(() => writer.Write(A<byte[]>.Ignored, A<int>.Ignored, A<int>.Ignored))
                .Invokes(new Action<byte[], int, int>(buffer.Write));
            A.CallTo(() => writer.CurrentOffset)
                .ReturnsLazily(x => (int)buffer.Position);

            return new SerializationContext(null) { StreamWriter = writer };
        }

        private static List<int> ArrayOfLength(int length)
        {
            var result = new List<int>();

            for (var i = 0; i < length; i++)
            {
                result.Add(i);
            }

            return result;
        }
    }
}
