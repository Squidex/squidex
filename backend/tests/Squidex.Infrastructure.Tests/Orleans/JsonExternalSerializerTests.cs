// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using FakeItEasy;
using Orleans.Serialization;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

#pragma warning disable MA0060 // The value returned by Stream.Read/Stream.ReadAsync is not used

namespace Squidex.Infrastructure.Orleans
{
    public class JsonExternalSerializerTests
    {
        public JsonExternalSerializerTests()
        {
            J.DefaultSerializer = TestUtils.DefaultSerializer;
        }

        [Fact]
        public void Should_not_copy_null()
        {
            var source = (string?)null;

            var clone = J<int>.Copy(source, null);

            Assert.Null(clone);
        }

        [Fact]
        public void Should_copy_null_json()
        {
            var source = new J<List<int>?>(null);

            var clone = (J<List<int>>)J<object>.Copy(source, null)!;

            Assert.Null(clone.Value);
        }

        [Fact]
        public void Should_not_copy_immutable_values()
        {
            var source = new List<int> { 1, 2, 3 }.AsJ();

            var copy = (J<List<int>>)J<object>.Copy(source, null)!;

            Assert.Same(source.Value, copy.Value);
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
            using (var buffer = new MemoryStream())
            {
                J<object>.Serialize(J.Of(value), CreateWriter(buffer), typeof(T));

                buffer.Position = 0;

                var copy = (J<T>)J<object>.Deserialize(typeof(J<T>), CreateReader(buffer))!;

                equals(copy.Value, value);

                Assert.NotSame(value, copy.Value);
            }
        }

        private static IDeserializationContext CreateReader(MemoryStream buffer)
        {
            var reader = A.Fake<IBinaryTokenStreamReader>();

            A.CallTo(() => reader.ReadByteArray(A<byte[]>._, A<int>._, A<int>._))
                .Invokes(new Action<byte[], int, int>((array, offset, length) => buffer.Read(array, offset, length)));
            A.CallTo(() => reader.CurrentPosition)
                .ReturnsLazily(x => (int)buffer.Position);
            A.CallTo(() => reader.Length)
                .ReturnsLazily(x => (int)buffer.Length);

            var context = A.Fake<IDeserializationContext>();

            A.CallTo(() => context.StreamReader)
                .Returns(reader);

            return context;
        }

        private static ISerializationContext CreateWriter(MemoryStream buffer)
        {
            var writer = A.Fake<IBinaryTokenStreamWriter>();

            A.CallTo(() => writer.Write(A<byte[]>._, A<int>._, A<int>._))
                .Invokes(new Action<byte[], int, int>(buffer.Write));
            A.CallTo(() => writer.CurrentOffset)
                .ReturnsLazily(x => (int)buffer.Position);

            var context = A.Fake<ISerializationContext>();

            A.CallTo(() => context.StreamWriter)
                .Returns(writer);

            return context;
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
