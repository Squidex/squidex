// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans.Serialization;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

#pragma warning disable MA0060 // The value returned by Stream.Read/Stream.ReadAsync is not used

namespace Squidex.Infrastructure.Orleans
{
    public class JsonExternalSerializerTests
    {
        private readonly IExternalSerializer serializer = new JsonSerializer(TestUtils.DefaultSerializer);

        [Fact]
        public void Should_not_copy_null()
        {
            var source = (string?)null;

            var clone = serializer.DeepCopy(source, null);

            Assert.Null(clone);
        }

        [Fact]
        public void Should_not_copy_values()
        {
            var source = new List<int> { 1, 2, 3 };

            var copy = serializer.DeepCopy(source, null)!;

            Assert.Same(source, copy);
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

        private void SerializeAndDeserialize<T>(T value, Action<T, T> assertEquals) where T : class
        {
            using (var buffer = new MemoryStream())
            {
                var jsonWriter = CreateWriter(buffer);
                var jsonReader = CreateReader(buffer);

                serializer.Serialize(value, jsonWriter, typeof(T));

                buffer.Position = 0;

                var deserialized = (T)serializer.Deserialize(typeof(T), jsonReader)!;

                assertEquals(deserialized, value);
                Assert.NotSame(value, deserialized);
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
